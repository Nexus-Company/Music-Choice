using Microsoft.EntityFrameworkCore;
using Nexus.Music.Choice.Worker.Entities;

namespace Nexus.Music.Choice.Worker.Services;

public class InteractContext : DbContext
{
    public DbSet<Interaction> Interactions { get; set; }

    public InteractContext()
    {
    }

    public InteractContext(DbContextOptions<DbContext> options)
        : base(options)
    {

    }
}