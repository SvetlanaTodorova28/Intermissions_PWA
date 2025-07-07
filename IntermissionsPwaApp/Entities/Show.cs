public class Show
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Intermission { get; set; } = string.Empty; // pauze (bv. "15:30")
    public string End { get; set; } = string.Empty;           // eindtijd (bv. "17:15")
    public DateTime KdmExpires { get; set; }                  // exacte vervaldatum
}