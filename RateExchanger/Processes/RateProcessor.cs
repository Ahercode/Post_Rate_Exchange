using System.Text.Json;
using Microsoft.Data.SqlClient;
using RateExchanger.Models;

namespace RateExchanger.Processes;

public class RateProcessor
{
    
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    public RateProcessor(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
   
        
   
    public async Task ProcessRates( CancellationToken stoppingToken)
    {
        var usdConnectionString = _configuration.GetConnectionString("USDConnection");
        var ghcConnectionString = _configuration.GetConnectionString("GHCConnection");
        var url = _configuration["ApiSettings:apiUrl"];
        var key = _configuration["ApiSettings:key"];
        var host = _configuration["ApiSettings:host"];
        var httpClient = new HttpClient();

         // for rapid api
        httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", key);
        httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", host);
        
          var response = await httpClient.GetAsync(url, stoppingToken);

            if (response.IsSuccessStatusCode)
            {

                // Deserialize the response
                var content = await response.Content.ReadAsStringAsync();
                
                JsonDocument document = JsonDocument.Parse(content);
                
                var data = JsonSerializer.Deserialize<ApiResponse>(content);
                // log the data
                _logger.LogInformation("Rate Time is {data}", data.rates.GHS);
                _logger.LogInformation("Rate Time is {data}", data.time_update.time_utc);
                
                using (SqlConnection connection = new SqlConnection(usdConnectionString))
                {
                    await connection.OpenAsync(stoppingToken);
                
                    // I need current date
                    DateTime date = DateTime.Now;
                
                    // convert the date to decimal
                    decimal audtDate = Convert.ToDecimal(date.ToString("yyyyMMdd"));
                    // get the current time and convert it to decimal
                    decimal audtTime = Convert.ToDecimal(date.ToString("HHmmssff"));
                
                    string dateTimeString = data.time_update.time_utc;
                    DateTime dateTime = DateTime.Parse(dateTimeString);
                    string dateString = dateTime.ToString("yyyyMMdd");
                    decimal dateDecimal = Convert.ToDecimal(dateString);
                
                    decimal rateDate = dateDecimal;
                    decimal rate = Convert.ToDecimal(data.rates.GHS);
                
                    _logger.LogInformation("Rate date is  {date}", rateDate);
                
                    string columns =
                        "HOMECUR, RATETYPE, SOURCECUR, RATEDATE, AUDTDATE, AUDTTIME, AUDTUSER, AUDTORG, RATE, SPREAD,DATEMATCH,RATEOPER";
                    string values =
                        $"'GHC','SP','USD','{rateDate}', '{audtDate}', '{audtTime}', 'ADMIN', 'SYSUSD', '{rate}', '0.0000000','3','1'";
                    using (SqlCommand command =
                           new SqlCommand($"INSERT INTO [SYSTEMUSD].[dbo].[CSCRD] ({columns}) VALUES ({values})",
                               connection))
                    {
                        await command.ExecuteNonQueryAsync(stoppingToken);
                    }
                }
                
                using (SqlConnection connection = new SqlConnection(ghcConnectionString))
                {
                    await connection.OpenAsync(stoppingToken);
                
                    // I need current date
                    DateTime date = DateTime.Now;
                
                    // convert the date to decimal
                    decimal audtDate = Convert.ToDecimal(date.ToString("yyyyMMdd"));
                    // get the current time and convert it to decimal
                    decimal audtTime = Convert.ToDecimal(date.ToString("HHmmssff"));
                
                    string dateTimeString = data.time_update.time_utc;
                    DateTime dateTime = DateTime.Parse(dateTimeString);
                    string dateString = dateTime.ToString("yyyyMMdd");
                    decimal dateDecimal = Convert.ToDecimal(dateString);
                
                    decimal rateDate = dateDecimal;
                    decimal rate = Convert.ToDecimal(data.rates.GHS);
                
                    _logger.LogInformation("Rate date is  {date}", rateDate);
                
                    string columns =
                        "HOMECUR, RATETYPE, SOURCECUR, RATEDATE, AUDTDATE, AUDTTIME, AUDTUSER, AUDTORG, RATE, SPREAD,DATEMATCH,RATEOPER";
                    string values =
                        $"'USD','SP','GHC ','{rateDate}', '{audtDate}', '{audtTime}', 'ADMIN', 'SYSTEM', '{rate}', '0.0000000','3','2'";
                    using (SqlCommand command =
                           new SqlCommand($"INSERT INTO [SYSTEM].[dbo].[CSCRD] ({columns}) VALUES ({values})",
                               connection))
                    {
                        await command.ExecuteNonQueryAsync(stoppingToken);
                    }
                }
            }
    }
}