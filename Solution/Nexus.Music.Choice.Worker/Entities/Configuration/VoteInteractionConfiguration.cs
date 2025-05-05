using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Music.Choice.Worker.Services;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.Entities.Configuration;

public class VoteInteractionConfiguration : IEntityTypeConfiguration<VoteInteraction>
{
    public void Configure(EntityTypeBuilder<VoteInteraction> builder)
    {
        builder.Property(v => v.Data)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<string>(v, (JsonSerializerOptions?)null)
            );
    }
}
