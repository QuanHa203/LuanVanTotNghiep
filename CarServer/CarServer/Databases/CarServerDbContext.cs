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

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<Esp32Camera> Esp32Cameras { get; set; }

    public virtual DbSet<Esp32Control> Esp32Controls { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>(entity =>
        {
            entity.ToTable("Car");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Esp32Camera).WithOne(p => p.Car)
                .HasForeignKey<Car>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Car_Esp32Camera");

            entity.HasOne(d => d.Esp32Control).WithOne(p => p.Car)
                .HasForeignKey<Car>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
