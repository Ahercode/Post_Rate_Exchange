using System.Text.Json;
using Microsoft.Data.SqlClient;
using RateExchanger.Models;
using RateExchanger.Processes;

namespace RateExchanger;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RateProcessor _rateProcessor;
    

    public Worker(ILogger<Worker> logger, RateProcessor rateProcessor)
    {
        _logger = logger;
        _rateProcessor = rateProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _rateProcessor.ProcessRates(stoppingToken);
            await Task.Delay(60000, stoppingToken);
            
        }
    }
    
}