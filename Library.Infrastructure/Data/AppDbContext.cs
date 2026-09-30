using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<BookLoan> Loans { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка таблицы
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");

            entity.HasKey(b => b.Id);

            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(b => b.ISBN)
                .IsRequired()
                .HasMaxLength(20);

            // Уникальный индекс для ISBN
            entity.HasIndex(b => b.ISBN).IsUnique();

            entity.Property(b => b.Genre)
                .HasMaxLength(100);

            entity.HasQueryFilter(b => !b.IsDeleted);

            entity.Property(b => b.IsDeleted).HasDefaultValue(false);
        });

        // Настройка User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Настройка BookLoan
        modelBuilder.Entity<BookLoan>(entity =>
        {
            entity.ToTable("BookLoans");
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Fine).HasPrecision(18, 2);

            // Связь с User (один ко многим)
            entity.HasOne(l => l.User)
                  .WithMany(u => u.Loans)
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Связь с Book (один ко многим)
            entity.HasOne(l => l.Book)
                  .WithMany() // у Book пока нет коллекции, можем добавить позже
                  .HasForeignKey(l => l.BookId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Token).IsRequired().HasMaxLength(200);
            entity.HasIndex(t => t.Token).IsUnique();

            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}