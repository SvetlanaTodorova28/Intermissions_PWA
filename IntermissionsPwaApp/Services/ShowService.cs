// ShowService.cs
using System.Text;
using System.Text.Json;
using IntermissionsPwaApp.Entities;
using Microsoft.JSInterop;

namespace IntermissionsPwaApp.Services;

public class ShowService : IShowService
{
    private const string StorageKey = "shows";
    private const string ArchiveKey = "archivedShows";
    private readonly IJSRuntime _js;

    public ShowService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<List<Show>> GetAllAsync()
    {
        return await GetFromStorageAsync(StorageKey);
    }

    public async Task<Show?> GetByIdAsync(string id)
    {
        var shows = await GetAllAsync();
        return shows.FirstOrDefault(s => s.Id == id);
    }

    public async Task CreateAsync(Show newShow)
    {
        var shows = await GetAllAsync();
        shows.Add(newShow);
        await SaveToStorageAsync(StorageKey, shows);
    }

    public async Task UpdateAsync(Show updatedShow)
    {
        var shows = await GetAllAsync();
        var index = shows.FindIndex(s => s.Id == updatedShow.Id);
        if (index >= 0)
        {
            shows[index] = updatedShow;
            await SaveToStorageAsync(StorageKey, shows);
        }
    }

    public async Task DeleteAsync(string id)
    {
        var shows = await GetAllAsync();
        shows = shows.Where(s => s.Id != id).ToList();
        await SaveToStorageAsync(StorageKey, shows);
    }

    public async Task ArchiveShowAsync(string id)
    {
        var shows = await GetAllAsync();
        var showToArchive = shows.FirstOrDefault(s => s.Id == id);
        if (showToArchive == null) return;

        shows = shows.Where(s => s.Id != id).ToList();
        await SaveToStorageAsync(StorageKey, shows);

        var archived = await GetFromStorageAsync(ArchiveKey);
        archived.Add(showToArchive);
        await SaveToStorageAsync(ArchiveKey, archived);
    }

    public async Task UnarchiveShowAsync(string id)
    {
        var archived = await GetFromStorageAsync(ArchiveKey);
        var showToRestore = archived.FirstOrDefault(s => s.Id == id);
        if (showToRestore == null) return;

        archived = archived.Where(s => s.Id != id).ToList();
        await SaveToStorageAsync(ArchiveKey, archived);

        var active = await GetFromStorageAsync(StorageKey);
        active.Add(showToRestore);
        await SaveToStorageAsync(StorageKey, active);
    }

    public async Task UpdateBadgeAsync()
    {
        var shows = await GetAllAsync();
        var soonExpiringCount = shows.Count(s => s.KdmExpires <= DateTime.Today.AddDays(3));
        await _js.InvokeVoidAsync("console.log", $"\uD83C\uDF1F Badge count: {soonExpiringCount}");

        if (soonExpiringCount > 0)
        {
            var lastChecked = await _js.InvokeAsync<string>("localStorage.getItem", "lastBadgeCheck");
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            if (lastChecked != today)
            {
                await _js.InvokeVoidAsync("badgeHelper.setBadge");
                await _js.InvokeVoidAsync("localStorage.setItem", "lastBadgeCheck", today);
            }
        }
        else
        {
            await _js.InvokeVoidAsync("badgeHelper.clearBadge");
            await _js.InvokeVoidAsync("localStorage.removeItem", "lastBadgeCheck");
        }
    }

    public async Task ExportToCsvAsync()
    {
        var shows = await GetAllAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Titel,Pauze,Einde,KDM-vervalt");

        foreach (var show in shows)
        {
            var line = $"\"{Escape(show.Title)}\",\"{Escape(show.Intermission)}\",\"{Escape(show.End)}\",\"{show.KdmExpires:yyyy-MM-dd}\"";
            csv.AppendLine(line);
        }

        await _js.InvokeVoidAsync("downloadHelper.downloadCsv", csv.ToString(), "shows_export.csv");
    }


    public async Task ImportFromCsvAsync(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1);
        var shows = new List<Show>();

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length >= 4)
            {
                shows.Add(new Show
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = parts[0].Trim('"'),
                    Intermission = parts[1].Trim('"'),
                    End = parts[2].Trim('"'),
                    KdmExpires = DateTime.TryParse(parts[3].Trim('"'), out var date) ? date : DateTime.Today
                });
            }
        }

        await SaveToStorageAsync(StorageKey, shows);
    }

    private static string Escape(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "" : "\"" + value.Replace("\"", "\"\"") + "\"";


    private async Task<List<Show>> GetFromStorageAsync(string key)
    {
        var json = await _js.InvokeAsync<string>("localStorage.getItem", key);
        return string.IsNullOrWhiteSpace(json)
            ? new List<Show>()
            : JsonSerializer.Deserialize<List<Show>>(json) ?? new List<Show>();
    }

    private async Task SaveToStorageAsync(string key, List<Show> shows)
    {
        var json = JsonSerializer.Serialize(shows);
        await _js.InvokeVoidAsync("localStorage.setItem", key, json);
    }
}
