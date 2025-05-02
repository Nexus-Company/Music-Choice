namespace Nexus.Music.Choice.Worker.Workers.MusicPipe;

interface IPipeStream : IDisposable
{
    void Start();
    void Stop();
}