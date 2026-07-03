using Microsoft.EntityFrameworkCore;
using PoolerBotCS;
using PoolerBotCS.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<IrcBotService>();
builder.Services.AddHostedService<DiscordBotService>();

builder.Services.AddDbContext<PoolerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();