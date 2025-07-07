using System.Text.Json;
using Microsoft.JSInterop;
using IntermissionsPwaApp.Services;
namespace IntermissionsPwaApp.Services;

public class ShowService : IShowService
{
    private const string StorageKey = "shows";
    private readonly IJSRuntime _js;

    public ShowService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<List<Show>> GetAllAsync()
    {
        var json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
        return string.IsNullOrWhiteSpace(json)
            ? new List<Show>()
            : JsonSerializer.Deserialize<List<Show>>(json) ?? new List<Show>();
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
        await SaveAllAsync(shows);
    }

    public async Task UpdateAsync(Show updatedShow)
    {
        var shows = await GetAllAsync();
        var index = shows.FindIndex(s => s.Id == updatedShow.Id);
        if (index >= 0)
        {
            shows[index] = updatedShow;
            await SaveAllAsync(shows);
        }
    }

    public async Task DeleteAsync(string id)
    {
        var shows = await GetAllAsync();
        var updated = shows.Where(s => s.Id != id).ToList();
        await SaveAllAsync(updated);
    }

    private async Task SaveAllAsync(List<Show> shows)
    {
        var json = JsonSerializer.Serialize(shows);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}