using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Models;

namespace WebPtichka.Data;

public partial class PtichkaContext : DbContext
{
    public PtichkaContext()
    {
    }

    public PtichkaContext(DbContextOptions<PtichkaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ContractService> ContractServices { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Hall> Halls { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ANZHELA-PC\\SQLEXPRESS; Database=Ptichka;Trusted_Connection=True; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ContractNumber).HasMaxLength(30);
            entity.Property(e => e.ContractStatus).HasMaxLength(30);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.HallId).HasColumnName("HallID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Client).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contracts_Clients");

            entity.HasOne(d => d.Employee).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contracts_Employees");

            entity.HasOne(d => d.Hall).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.HallId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contracts_Halls");
        });

        modelBuilder.Entity<ContractService>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractServices)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContractServices_Contracts");

            entity.HasOne(d => d.Service).WithMany(p => p.ContractServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContractServices_Services");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.HallName).HasMaxLength(100);
            entity.Property(e => e.HallStatus).HasMaxLength(30);
            entity.Property(e => e.PricePerHour).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(30);
            entity.Property(e => e.InvoiceStatus).HasMaxLength(30);

            entity.HasOne(d => d.Contract).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_Contracts");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.PaymentMethod).HasMaxLength(30);
            entity.Property(e => e.PaymentStatus).HasMaxLength(30);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Invoices");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.HallId).HasColumnName("HallID");
            entity.Property(e => e.ScheduleStatus).HasMaxLength(30);

            entity.HasOne(d => d.Contract).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedules_Contracts");

            entity.HasOne(d => d.Hall).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.HallId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedules_Halls");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ServiceName).HasMaxLength(100);
            entity.Property(e => e.ServicePrice).HasColumnType("decimal(10, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
