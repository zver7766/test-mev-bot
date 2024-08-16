using DexCexMevBot.AppInitializer;
using DexCexMevBot.Config.Extensions;
using DexCexMevBot.Modules.Estimator.Extensions;
using DexCexMevBot.Modules.Mempool.Extensions;
using DexCexMevBot.Modules.Orberbooks.Extensions;
using DexCexMevBot.Modules.Reserves.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.AddConfiguration();
builder.AddNetworkName();
builder.AddTelegramBot();
builder.Services.AddHostedService<AppInitializer>();

builder.Services.AddReserves();
builder.Services.AddOrderbooks();
builder.Services.AddMempool();
builder.Services.AddEstimator();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();