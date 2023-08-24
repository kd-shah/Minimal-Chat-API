using ChatApi.Model;
using Microsoft.EntityFrameworkCore;


namespace ChatApi.Context
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { 
        }

        public DbSet<Model.User> Users { get; set; }
        public DbSet<Model.Message> Messages { get; set; }

        public DbSet<Model.Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.User>().ToTable("users");

            modelBuilder.Entity<Message>()
               .HasOne(m => m.sender)
               .WithMany(u => u.sentMessages)
               .HasForeignKey(m => m.senderId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.receiver)
                .WithMany(u => u.receivedMessages)
                .HasForeignKey(m => m.receiverId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
