using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TransactSmartFilter.API.Common;
using TransactSmartFilter.Application.Common;
using TransactSmartFilter.Application.Services.TransactionConsumer;
using TransactSmartFilter.Infrastructure.Common;
using TransactSmartFilter.Infrastructure.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/transaction.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

//Add Mapster
builder.Services.RegisterMapper();

// MassTransit configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionSearchConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => { });
        cfg.ReceiveEndpoint("transaction-search-queue", e =>
        {
            e.ConfigureConsumer<TransactionSearchConsumer>(context);
        });
    });
});

// Add Redis Cache
builder.Services.AddDistributedMemoryCache();

// Register Services in DI Container
builder.Services.RegisterServices(builder.Environment, builder.Configuration);

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Database Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    var env = services.GetRequiredService<IHostEnvironment>();
    var config = services.GetRequiredService<IConfiguration>();

    await DbSeeder.Initialize(context, env, config);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();