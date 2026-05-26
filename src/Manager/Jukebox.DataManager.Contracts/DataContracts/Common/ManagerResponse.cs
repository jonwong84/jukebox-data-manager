namespace Jukebox.DataManager.Contracts.DataContracts.Common
{
    public class ManagerResponse<T>
    {
        public T? Data { get; set; }
        public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}