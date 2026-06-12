namespace Jukebox.DataManager.Contracts.DataContracts.Genre;

public class GenreSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentGenreId { get; set; }
}