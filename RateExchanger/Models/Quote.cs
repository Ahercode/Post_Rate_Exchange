namespace RateExchanger.Models;

public class Quote
{
    public string base_currency { get; set; }
    public string quote_currency { get; set; }
    public string date_time { get; set; }
    public string bid { get; set; }
    public string ask { get; set; }
    public string midpoint { get; set; }
}