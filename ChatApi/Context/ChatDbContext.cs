using Microsoft.EntityFrameworkCore;


namespace ChatApi.Context
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { 
        }

        public DbSet<Model.User> Users { get; set; }
        public DbSet<Model.Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.User>().ToTable("users");
        }
    }
}
