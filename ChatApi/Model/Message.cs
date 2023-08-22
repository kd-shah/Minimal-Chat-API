namespace ChatApi.Model
{
    public class Message
    {
        public int id { get; set; }
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public string content { get; set; }
        public DateTime timestamp { get; set; }
    }
}
