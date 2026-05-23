namespace Jukebox.DataManager.Contracts.DataContracts.Common
{
    public class ManagerRequest<T>
    {
        public required T Data { get; set; }
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
        public required string UserId { get; set; }
    }
}