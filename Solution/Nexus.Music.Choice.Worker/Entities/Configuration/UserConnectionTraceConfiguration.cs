using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nexus.Music.Choice.Worker.Entities.Configuration;

public class UserConnectionTraceConfiguration : IEntityTypeConfiguration<UserConnectionTrace>
{
    public void Configure(EntityTypeBuilder<UserConnectionTrace> builder)
    {
        builder.HasKey(u => u.Id);
    }
}