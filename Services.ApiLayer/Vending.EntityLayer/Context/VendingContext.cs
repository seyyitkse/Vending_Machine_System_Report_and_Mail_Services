using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Services.ApiLayer.Vending.EntityLayer.Concrete;

namespace Services.ApiLayer.Vending.EntityLayer.Context
{
    public class VendingContext : IdentityDbContext<AppUser, AppRole, int>
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //optionsBuilder.UseSqlServer("server=MSI-Bravo\\SQL2022DEV;database=DbVending;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;encrypt=false");

        //    optionsBuilder.UseSqlServer("Server=185.188.131.208,14330;Database=DbVending;User Id=sa;Password=123456;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false;");

        //}
        private readonly IConfiguration _configuration;

        public VendingContext(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            Cache = cache;
        }

        public IMemoryCache Cache { get; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Bağlantı dizesini IConfiguration üzerinden al
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One Department can have many AppUsers
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.AppUsers)
                .HasForeignKey(u => u.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            modelBuilder.Entity<AppUser>()
                .ToTable("AspNetUsers", t => t.HasTrigger("trg_SetUserCode"));

            // One Category can have many Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            // One Brand can have many Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            // One Product can have many Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            //One AppUser can have many Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.AppUser)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserCode) // Keep as UserCode
                .HasPrincipalKey(u => u.UserCode) // Specify principal key
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            // One Vend can have many Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Vend)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VendId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            // VendProduct için ilişki tanımları
            modelBuilder.Entity<VendProduct>()
                .HasOne(vp => vp.Vend)
                .WithMany(v => v.VendProducts) // Correct navigation property
                .HasForeignKey(vp => vp.VendId);

            modelBuilder.Entity<VendProduct>()
                .HasOne(vp => vp.Product)
                .WithMany(p => p.VendProducts) // Correct navigation property
                .HasForeignKey(vp => vp.ProductId);

        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Vend> Vends { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Order> Orders { get; set; } // Add Orders DbSet
        public DbSet<VendProduct> VendProducts { get; set; } // Add VendProduct DbSet
        public DbSet<Log> Logs { get; set; }
    }
}

