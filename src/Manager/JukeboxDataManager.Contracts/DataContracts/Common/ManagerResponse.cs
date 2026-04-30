namespace Jukebox.DataManager.Contracts.DataContracts.Common
{
    public class ManagerResponse<T>
    {
        public required T Data { get; set; }
        public DateTime ResponseTime { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}