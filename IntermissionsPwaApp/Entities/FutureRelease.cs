using System;

namespace IntermissionsPwaApp.Entities
{
    public class FutureRelease
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public ReleaseType Type { get; set; }
        public DateTime ReleaseDate { get; set; } = DateTime.Today;
    }
}