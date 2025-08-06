using System.Text.Json;
using IntermissionsPwaApp.Entities;
using Microsoft.JSInterop;

namespace IntermissionsPwaApp.Services;

public class FutureReleaseService : IFutureReleaseService
{
    private const string StorageKey = "futureReleases";
    private readonly IJSRuntime _js;

    public FutureReleaseService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<List<FutureRelease>> GetAllAsync()
    {
        return await GetFromStorageAsync();
    }

    public async Task<FutureRelease?> GetByIdAsync(string id)
    {
        var releases = await GetAllAsync();
        return releases.FirstOrDefault(r => r.Id == id);
    }

    public async Task AddAsync(FutureRelease release)
    {
        Console.WriteLine($"🔄 AddAsync gestart voor: {release.Title}");

        var releases = await GetFromStorageAsync();
        Console.WriteLine($"📦 Releases in storage voor toevoegen: {releases.Count}");

        releases.Add(release);

        await SaveToStorageAsync(releases);

        var afterSave = await GetFromStorageAsync();
        Console.WriteLine($"✅ Releases na toevoegen: {afterSave.Count}");
    }


    public async Task UpdateAsync(FutureRelease updatedRelease)
    {
        var releases = await GetFromStorageAsync();
        var index = releases.FindIndex(r => r.Id == updatedRelease.Id);
        if (index >= 0)
        {
            releases[index] = updatedRelease;
            await SaveToStorageAsync(releases);
        }
    }

    public async Task DeleteAsync(string id)
    {
        var releases = await GetFromStorageAsync();
        releases = releases.Where(r => r.Id != id).ToList();
        await SaveToStorageAsync(releases);
    }

    private async Task<List<FutureRelease>> GetFromStorageAsync()
    {
        var json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
        return string.IsNullOrWhiteSpace(json)
            ? new List<FutureRelease>()
            : JsonSerializer.Deserialize<List<FutureRelease>>(json) ?? new List<FutureRelease>();
    }

    private async Task SaveToStorageAsync(List<FutureRelease> releases)
    {
        var json = JsonSerializer.Serialize(releases);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
  
    public async Task MoveToShowAsync(string id)
    {
        var futureReleases = await GetFromStorageAsync();
        var releaseToMove = futureReleases.FirstOrDefault(r => r.Id == id);
        if (releaseToMove == null) return;
        
        await DeleteAsync(releaseToMove.Id); 

        var newShow = new Show
        {
            Id = Guid.NewGuid().ToString(),
            Title = releaseToMove.Title,
            Intermission = "",
            End = "",
            KdmExpires = releaseToMove.ReleaseDate
        };

        var showJson = await _js.InvokeAsync<string>("localStorage.getItem", "shows");
        var shows = string.IsNullOrWhiteSpace(showJson)
            ? new List<Show>()
            : JsonSerializer.Deserialize<List<Show>>(showJson) ?? new List<Show>();

        shows.Add(newShow);
        var updatedJson = JsonSerializer.Serialize(shows);
        await _js.InvokeVoidAsync("localStorage.setItem", "shows", updatedJson); 
    }

}