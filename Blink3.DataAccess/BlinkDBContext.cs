using Blink3.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess;

public class BlinkDbContext(DbContextOptions<BlinkDbContext> options) : DbContext(options)
{
    public DbSet<BlinkGuild> BlinkGuilds { get; set; }
}