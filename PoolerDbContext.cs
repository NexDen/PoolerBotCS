using Microsoft.EntityFrameworkCore;
using PoolerBotCS.Models;

namespace PoolerBotCS;

public class PoolerDbContext(DbContextOptions<PoolerDbContext> options) : DbContext(options)
{
    public DbSet<BanchoLobby> BanchoLobbies { get; set; }
    public DbSet<BanchoPlayer> BanchoPlayers { get; set; }
}