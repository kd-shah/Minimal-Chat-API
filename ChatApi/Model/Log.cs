namespace ChatApi.Model
{
    public class Log
    {
        public int logId { get; set; }
        public string ipAddress { get; set; }
        public string requestBody { get; set; }
        public DateTime timeStamp { get; set; }
    }
}
