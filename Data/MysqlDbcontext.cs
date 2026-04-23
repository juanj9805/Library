using library.Models;
using Microsoft.EntityFrameworkCore;

namespace library.Data;

public class MysqlDbcontext : DbContext
{
    public MysqlDbcontext(DbContextOptions<MysqlDbcontext> options) : base(options)
    {
    }


    public DbSet<Book> books { get; set; }
    public DbSet<User> users { get; set; }
    public DbSet<Loan> loans { get; set; }
    public DbSet<LoanBook> loanBooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(book =>
        {
            book.Property(b => b.Title).HasMaxLength(45).IsRequired();
            book.Property(b => b.Author).IsRequired().HasMaxLength(45);
            book.Property(b => b.Status).IsRequired();
        });

        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Name).HasMaxLength(45).IsRequired();
            user.Property(u => u.DNI).IsRequired().HasMaxLength(45);
            user.Property(u => u.Lastname).IsRequired().HasMaxLength(45);
        });

        modelBuilder.Entity<Loan>(loan =>
        {
            loan.Property(l => l.UserId).IsRequired();
            loan.Property(l => l.DateEnd).IsRequired();
            loan.HasOne(l => l.User)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoanBook>(loanBook =>
        {
            loanBook.HasKey(lb => new { lb.LoanId, lb.BookId });

            loanBook.Property(lb => lb.LoanId).IsRequired();
            loanBook.Property(lb => lb.BookId).IsRequired();
            
            loanBook.HasOne(lb => lb.Book)
                .WithMany(b => b.LoanBooks)
                .HasForeignKey(lb => lb.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            loanBook.HasOne(lb => lb.Loan)
                .WithMany(b => b.LoanBooks)
                .HasForeignKey(lb => lb.LoanId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}