using Microsoft.EntityFrameworkCore;

namespace Nexus.Music.Choice.Worker.Services;

public class InteractContext : DbContext
{
    public InteractContext()
    {
    }

    public InteractContext(DbContextOptions<DbContext> options)
        : base(options)
    {

    }
}