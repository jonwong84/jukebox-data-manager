namespace Jukebox.DataManager.Contracts.DataContracts.Common
{
    public class ManagerRequest<T>
    {
        public required T Data { get; set; }
        public DateTime RequestTime { get; set; }
        public string UserId { get; set; }
    }
}