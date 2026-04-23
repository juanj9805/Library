namespace library.Models;

public class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime DateStart { get; set; }= DateTime.Now;
    public DateTime DateEnd { get; set; }
    public List<LoanBook> LoanBooks { get; set; } = [];
}