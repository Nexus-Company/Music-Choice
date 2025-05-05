using Microsoft.EntityFrameworkCore;
using Nexus.Music.Choice.Worker.Entities;

namespace Nexus.Music.Choice.Worker.Services;

public class InteractContext : DbContext
{
    public DbSet<VoteInteraction> VoteInteractions { get; set; }
    public DbSet<ActionExecuted> ActionsExecuted { get; set; }
    public DbSet<TrackFeedback> TrackFeedbacks { get; set; }

    public InteractContext()
    {
    }

    public InteractContext(DbContextOptions<DbContext> options)
        : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseInMemoryDatabase("InteractionsDB");

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InteractContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}