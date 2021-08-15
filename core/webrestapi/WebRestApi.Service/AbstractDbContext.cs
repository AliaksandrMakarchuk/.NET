using Microsoft.EntityFrameworkCore;
using WebRestApi.Service.Models;

namespace WebRestApi.Service
{
    public abstract class AbstractDbContext : DbContext
    {
        public AbstractDbContext(DbContextOptions<AbstractDbContext> options) : base(options) { }

        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }

        private static Role _adminRole;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<Message>()
            //     .HasOne(m => m.Sender)
            //     .WithMany(u => u.SendMessages)
            //     .HasForeignKey(m => m.SenderId)
            //     .OnDelete(DeleteBehavior.SetNull);


            var adminEmail = "admin@mail.ru";
            var adminPassword = "123456";

            // добавляем роли
            InitializeRoles(modelBuilder);

            var adminUser = new User
            {
                Id = 1,
                Email = adminEmail,
                Password = adminPassword,
                RoleId = _adminRole.Id,
                FirstName = "Администратор",
                LastName = string.Empty
            };
            modelBuilder.Entity<User>().HasData(new User[] { adminUser });

            base.OnModelCreating(modelBuilder);
        }

        private static void InitializeRoles(ModelBuilder modelBuilder)
        {
            _adminRole = new Role
            {
                Id = 1,
                Name = UserRole.ADMIN.RoleName
            };
            var userRole = new Role
            {
                Id = 2,
                Name = UserRole.USER.RoleName
            };

            modelBuilder.Entity<Role>().HasData(new Role[] { _adminRole, userRole });
        }
    }
}