// ArchivedShowService.cs
using System.Text;
using System.Text.Json;
using IntermissionsPwaApp.Entities;
using Microsoft.JSInterop;

namespace IntermissionsPwaApp.Services;

public class ArchivedShowService : IArchivedShowService
{
    private const string ArchiveKey = "archivedShows";
    private readonly IJSRuntime _js;

    public ArchivedShowService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<List<Show>> GetAllAsync()
    {
        return await GetFromStorageAsync();
    }

    public async Task AddAsync(Show newShow)
    {
        var shows = await GetFromStorageAsync();
        shows.Add(newShow);
        await SaveToStorageAsync(shows);
    }

    public async Task DeleteAsync(string id)
    {
        var shows = await GetFromStorageAsync();
        shows = shows.Where(s => s.Id != id).ToList();
        await SaveToStorageAsync(shows);
    }

    public async Task UnarchiveShowAsync(string id)
    {
        var shows = await GetFromStorageAsync();
        var showToRestore = shows.FirstOrDefault(s => s.Id == id);
        if (showToRestore == null) return;

        shows = shows.Where(s => s.Id != id).ToList();
        await SaveToStorageAsync(shows);

        var activeJson = await _js.InvokeAsync<string>("localStorage.getItem", "shows");
        var active = string.IsNullOrWhiteSpace(activeJson)
            ? new List<Show>()
            : JsonSerializer.Deserialize<List<Show>>(activeJson) ?? new List<Show>();

        active.Add(showToRestore);
        var newActiveJson = JsonSerializer.Serialize(active);
        await _js.InvokeVoidAsync("localStorage.setItem", "shows", newActiveJson);
    }

    public async Task ExportToCsvAsync()
    {
        var shows = await GetFromStorageAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Titel,Pauze,Einde,KDM-vervalt");

        foreach (var show in shows)
        {
            csv.AppendLine($"\"{Escape(show.Title)}\",\"{Escape(show.Intermission)}\",\"{Escape(show.End)}\",\"{show.KdmExpires:yyyy-MM-dd}\"");
        }

        await _js.InvokeVoidAsync("downloadHelper.downloadCsv", csv.ToString(), "archived_shows.csv");
    }

    public async Task ImportFromCsvAsync(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1);
        var shows = new List<Show>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = ParseCsvLine(line);
            if (parts.Length >= 4)
            {
                shows.Add(new Show
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = parts[0].Trim(),
                    Intermission = parts[1].Trim(),
                    End = parts[2].Trim(),
                    KdmExpires = DateTime.TryParse(parts[3].Trim(), out var date) ? date : DateTime.Today
                });
            }
        }

        await SaveToStorageAsync(shows);
    }

    private async Task<List<Show>> GetFromStorageAsync()
    {
        var json = await _js.InvokeAsync<string>("localStorage.getItem", ArchiveKey);
        return string.IsNullOrWhiteSpace(json)
            ? new List<Show>()
            : JsonSerializer.Deserialize<List<Show>>(json) ?? new List<Show>();
    }

    private async Task SaveToStorageAsync(List<Show> shows)
    {
        var json = JsonSerializer.Serialize(shows);
        await _js.InvokeVoidAsync("localStorage.setItem", ArchiveKey, json);
    }

    private string Escape(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "" : "\"" + value.Replace("\"", "\"\"") + "\"";

    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        bool inQuotes = false;
        var value = new StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (c == ',' && !inQuotes)
            {
                values.Add(value.ToString());
                value.Clear();
                continue;
            }
            value.Append(c);
        }

        values.Add(value.ToString());
        return values.ToArray();
    }
}
