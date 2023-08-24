using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApi.Model
{
    public class Message
    {
        public int messageId { get; set; }
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public string content { get; set; }
        public DateTime timestamp { get; set; }

        [ForeignKey("senderId")]
        public virtual User sender { get; set; }
        [ForeignKey("receiverId")]
        public virtual User receiver { get; set; }
    }
}
