using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Nexus.Music.Choice.Worker.Entities.Configuration;

public class ActionExecutedConfiguration : IEntityTypeConfiguration<ActionExecuted>
{
    public void Configure(EntityTypeBuilder<ActionExecuted> builder)
    {
        builder.Property(v => v.Data)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<string>(v, (JsonSerializerOptions?)null)
            );
    }
}