using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem_DBFirst.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Rental> Rentals { get; set; }
    public virtual DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=Default");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            // KRİTİK GÜNCELLEME: CustomerID'nin otomatik artacağını belirttik
            entity.Property(e => e.CustomerId)
                .HasColumnName("CustomerID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);
            // KRİTİK GÜNCELLEME: PaymentId'nin otomatik artacağını belirttik
            entity.Property(e => e.PaymentId)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);

            entity.HasOne(d => d.Rental).WithMany(p => p.Payments)
                .HasForeignKey(d => d.RentalId)
                .HasConstraintName("FK_Payments_Rentals");
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.RentalId);

            // KRİTİK GÜNCELLEME: RentalID'nin otomatik artacağını belirttik
            entity.Property(e => e.RentalId)
                .HasColumnName("RentalID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.RentalStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

            // İlişkiler
            entity.HasOne(d => d.Customer).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Rentals_Customers");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_Rentals_Vehicles");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId);

            // KRİTİK GÜNCELLEME: VehicleID'nin otomatik artacağını belirttik
            entity.Property(e => e.VehicleId)
                .HasColumnName("VehicleID")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DailyPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PlateNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}