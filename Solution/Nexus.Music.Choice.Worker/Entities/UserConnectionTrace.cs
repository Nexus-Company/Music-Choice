using Nexus.Music.Choice.Worker.Base.Models.CommandData;

namespace Nexus.Music.Choice.Worker.Entities;

public class UserConnectionTrace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ConnectionState State { get; set; }
    public DateTime At { get; set; }
}