using Microsoft.EntityFrameworkCore;
using PoolerBotCS;
using PoolerBotCS.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IrcBotService>();
builder.Services.AddSingleton<IIrcBotService>(sp => sp.GetRequiredService<IrcBotService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<IrcBotService>());
//builder.Services.AddHostedService<DiscordBotService>();
builder.Services.AddTransient<IBanchoLobbyService, BanchoLobbyService>();
builder.Services.AddTransient<IBanchoPlayerService, BanchoPlayerService>();

builder.Services.AddDbContext<PoolerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/lobby", (string lobbyName, IBanchoLobbyService service) =>
{
    service.Make(lobbyName);
    return Results.Ok();
});

app.Run();