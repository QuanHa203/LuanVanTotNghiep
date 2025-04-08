using CarServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CarServer.Databases;

public partial class CarServerDbContext : DbContext
{
    public CarServerDbContext()
    {
    }

    public CarServerDbContext(DbContextOptions<CarServerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<Esp32Camera> Esp32Cameras { get; set; }

    public virtual DbSet<Esp32Control> Esp32Controls { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server = DESKTOP-3TKOLCA; UID = Guest; Password = 270603; Database = CarServerDb ;TrustServerCertificate = true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUser");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK_AppUser_Role");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.ToTable("Car");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Esp32Camera).WithOne(p => p.Car)
                .HasForeignKey<Car>(d => d.Id)
                .HasConstraintName("FK_Car_Esp32Camera");

            entity.HasOne(d => d.Esp32Control).WithOne(p => p.Car)
                .HasForeignKey<Car>(d => d.Id)
                .HasConstraintName("FK_Car_Esp32Control");
        });

        modelBuilder.Entity<Esp32Camera>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Esp32Cam__3214EC07BE06F7FA");

            entity.ToTable("Esp32Camera");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.LastSeen).HasColumnType("datetime");
        });

        modelBuilder.Entity<Esp32Control>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Esp32Con__3214EC073B8DD0A8");

            entity.ToTable("Esp32Control");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.LastSeen).HasColumnType("datetime");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
