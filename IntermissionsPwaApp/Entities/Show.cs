namespace IntermissionsPwaApp.Entities;

public class Show
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;

    public string Intermission { get; set; } = string.Empty;

    public string End { get; set; } = string.Empty;

    public DateTime KdmExpires { get; set; } = DateTime.Today;
    public DateTime ReleaseDate { get; set; } = DateTime.Today;
}