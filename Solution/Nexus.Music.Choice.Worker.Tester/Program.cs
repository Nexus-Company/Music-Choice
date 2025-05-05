using Nexus.Music.Choice.Worker.Conector;

namespace Nexus.Music.Choice.Worker.Tester;

class Program
{
    static async Task Main(string[] args)
    {
        using var connector = new PipeConector();

        connector.MessageReceived += (sender, message) =>
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("[RECEBIDO] ");
            Console.ResetColor();
            Console.WriteLine(message);
        };

        connector.Start();

        while (true)
        {
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                break;

            connector.SendMessage(input);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[ENVIADO] ");
            Console.ResetColor();
            Console.WriteLine(input);
        }

        Console.WriteLine("Encerrando conexão...");
    }
}