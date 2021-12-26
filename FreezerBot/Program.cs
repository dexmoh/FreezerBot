using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FreezerBot
{
    class Program
    {
        // Discord client.
        private readonly DiscordSocketClient _client;

        // Flag used to check if the program is still running.
        public static bool isRunning;

        // Our bot's prefix.
        private readonly string prefix;

        // ID value associated with master account.
        private readonly ulong masterId;

        Program()
        {
            isRunning = true;

            prefix = System.IO.File.ReadAllText(@"data\prefix.txt");
            masterId = Convert.ToUInt64(System.IO.File.ReadAllText(@"data\master.txt"));

            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;

            _client.SetGameAsync("AAAAAAAA", null, ActivityType.Listening);
        }

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, System.IO.File.ReadAllText(@"data\token.txt"));
            await _client.StartAsync();

            // Keep running the program until its told to shutdown.
            while (isRunning)
                await Task.Delay(3000);

            await _client.StopAsync();
            Thread.Sleep(3000);
        }

        private Task LogAsync(LogMessage msg)
        {
            // Log the messages to a file.
            StreamWriter sw = new StreamWriter(@"data\debug.log", true);
            sw.WriteLine(msg.ToString());
            sw.Close();

            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage msg)
        {
            string[] args = msg.Content.ToLower().Split(' ');
            int argsLen = args.Length;
            
            if (args[0] != prefix)
                return;

            if (msg.Author.IsBot)
                return;

            bool isMaster = false;
            if (msg.Author.Id == masterId)
                isMaster = true;

            await CommandHandler.HandleCommandAsync(msg, args, argsLen, prefix, isMaster);
        }
    }
}
