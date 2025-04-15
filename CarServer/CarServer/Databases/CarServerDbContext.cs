using System;
using System.Collections.Generic;
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

    public virtual DbSet<Accessory> Accessories { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<CarAccessory> CarAccessories { get; set; }

    public virtual DbSet<Esp32Camera> Esp32Cameras { get; set; }

    public virtual DbSet<Esp32Control> Esp32Controls { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accessory>(entity =>
        {
            entity.ToTable("Accessory");

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUser");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(200);
            entity.Property(e => e.PasswordResetTokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .UseCollation("Latin1_General_CS_AS");

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK_AppUser_Role");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.ToTable("Car");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Esp32Camera).WithOne(p => p.Car)
                .HasForeignKey<Car>(d => d.Id)
                .HasConstraintName("FK_Car_Esp32Camera");

            entity.HasOne(d => d.Esp32Control).WithOne(p => p.Car)
                .HasForeignKey<Car>(d => d.Id)
                .HasConstraintName("FK_Car_Esp32Control");
        });

        modelBuilder.Entity<CarAccessory>(entity =>
        {
            entity.HasKey(e => new { e.IdCar, e.IdAccessory });

            entity.ToTable("CarAccessory");

            entity.HasOne(d => d.Accessory).WithMany(p => p.CarAccessories)
                .HasForeignKey(d => d.IdAccessory)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarAccessory_Accessory");

            entity.HasOne(d => d.Car).WithMany(p => p.CarAccessories)
                .HasForeignKey(d => d.IdCar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarAccessory_Car");
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
