using Newtonsoft.Json;
using Nexus.Music.Choice.Worker.Conector;

namespace Nexus.Music.Choice.Worker.Tester;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var connector = new PipeConector();

        connector.MessageReceived += (sender, message) =>
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("[RECEBIDO] ");
            Console.ResetColor();
            Console.WriteLine(JsonConvert.SerializeObject(message));
        };

        connector.Start();

        CommandProcessor commandProcessor = new(connector, OnExit);

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            Console.ForegroundColor = ConsoleColor.White;

            string? input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                continue;

            await commandProcessor.ProcessAsync(input);
        }
    }

    private static void OnExit()
    {

    }
}