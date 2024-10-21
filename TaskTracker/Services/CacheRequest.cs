namespace TaskTracker.Services
{
    public class CacheRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int Minutes { get; set; }
    }
}
