using System.IO.Pipes;
using System.Text;

namespace Nexus.Music.Choice.Worker.Tester;


class Program
{
    static async Task Main(string[] args)
    {
        string pipeName = "Music-Choice-Worker";

        using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
        {
            try
            {
                Console.WriteLine("Conectando ao pipe...");
                await pipeClient.ConnectAsync();
                Console.WriteLine("Conectado ao pipe.");

                var readTask = Task.Run(() => ReadMessages(pipeClient));

                await SendMessagesAsync(pipeClient);

                await readTask;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao conectar no pipe: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    private static async Task SendMessagesAsync(NamedPipeClientStream pipeClient)
    {
        while (true)
        {
            string message = Console.ReadLine();
            if (string.IsNullOrEmpty(message))
                break;

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await pipeClient.WriteAsync(messageBytes, 0, messageBytes.Length);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[ENVIADO] ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }

    private static async Task ReadMessages(NamedPipeClientStream pipeClient)
    {
        Console.Clear();
        byte[] buffer = new byte[1024];
        StringBuilder messageBuilder = new StringBuilder();

        while (true)
        {
            try
            {
                int bytesRead = await pipeClient.ReadAsync(buffer);
                if (bytesRead == 0) break;

                string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                messageBuilder.Append(chunk);

                while (true)
                {
                    int newlineIndex = messageBuilder.ToString().IndexOf('\n');
                    if (newlineIndex == -1) break;  // Ainda não chegou uma mensagem completa

                    string completeMessage = messageBuilder.ToString(0, newlineIndex).Trim();
                    messageBuilder.Remove(0, newlineIndex + 1);  // Remove a mensagem já processada

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("[RECEBIDO] ");
                    Console.ResetColor();
                    Console.WriteLine(completeMessage);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao ler do pipe: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

}