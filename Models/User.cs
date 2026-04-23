namespace library.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }= string.Empty;
    public string Lastname { get; set; }= string.Empty;
    public string DNI { get; set; }= string.Empty;
    public List<Loan> Loans { get; set; } = [];
}
