namespace RateExchanger.Models;

public class ApiResponse
{
    // public List<Quote> quotes { get; set; }
    public Rate rates { get; set; }
    public RateTime time_update { get; set; }
    
}