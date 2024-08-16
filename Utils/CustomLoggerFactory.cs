using Destructurama;
using DexCexMevBot.Modules.Estimator.Models.Estimator;
using DexCexMevBot.Modules.Estimator.Models.Events;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;
using ILogger = Serilog.ILogger;

namespace DexCexMevBot.Utils;

public static class CustomLoggerFactory
{
    public static ILogger CreateGlobalLogger()
    {
        return CreateLogger();
    }

    public static ILogger CreateLogger(params string[] labels)
    {
        var labelsString = string.Join(' ', labels.Select(label => $"[{label}]"));

        var logger = new LoggerConfiguration()
            .MinimumLevel.Override("EasyNetQ", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("System", LogEventLevel.Error)
            .Destructure.UsingAttributes()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console(
                new ExpressionTemplate(
                    "[{@l:u3}] {Labels} - {@m}\n{#if @x is not null}{Labels} {@x}{#end}",
                    theme: TemplateTheme.Literate))
            .CreateLogger()
            .ForContext("Labels", labelsString);

        return logger;
    }
    
    
    public static ILogger CreateLogger(ArbitrageOpportunityDto coreTask)
    {
        var logger = CreateLogger(
            coreTask.TaskId.ToString(),
            coreTask.TargetToken,
            coreTask.ExchangeOperations.Last().ExchangeId);
        return logger;
    }
    
    public static ILogger CreateLogger(ArbitrageTaskLegacy task)
    {
        var logger = CreateLogger(
            task.TaskId.ToString(),
            task.TargetCurrency,
            task.ExchangeOperations.Last().ExchangeId);
        return logger;
    }
    
    public static ILogger CreateLogger(EstimateResultsLegacyDto estimate, Guid label)
    {
        var logger = CreateLogger(estimate.Tasks.First().TargetCurrency, label.ToString());
        return logger;
    }
}