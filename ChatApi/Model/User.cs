using System.ComponentModel.DataAnnotations;

namespace ChatApi.Model
{
    public class User
    {
        [Key]
        public int userId { get; set; }
        public string name { get; set; }
        public string email { get; set; }

        public string password { get; set; }
        public string token { get; set; }

        public virtual ICollection<Message>? sentMessages { get; set; } 
        public virtual  ICollection<Message>? receivedMessages { get; set; } 
    }   
}
