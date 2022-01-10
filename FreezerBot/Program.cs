using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FreezerBot;

class Program
{
    // Discord client.
    private readonly DiscordSocketClient _client;

    // Bot author's discord account ID.
    private readonly ulong _adminID;

    // Flag used to check if the program is still running.
    public static bool IsRunning;

    // Our bot's prefix.
    public static string Prefix;

    Program()
    {
        IsRunning = true;

        // Set bot's prefix.
        Prefix = "poss";

        if (File.Exists(@"data\prefix.txt"))
            Prefix = File.ReadAllText(@"data\prefix.txt");
        else
            Console.WriteLine("Couldn't find a 'prefix.txt' file inside data directory. Setting the bot's prefix to 'poss'.");

        _adminID = Convert.ToUInt64(File.ReadAllText(@"data\master.txt"));

        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += MessageReceivedAsync;

        _client.SetGameAsync("AAAAAAAA", null, ActivityType.Listening);
    }

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        await _client.LoginAsync(TokenType.Bot, File.ReadAllText(@"data\token.txt"));
        await _client.StartAsync();

        // Keep running the program until its told to shutdown.
        while (IsRunning)
            await Task.Delay(3000);

        await _client.StopAsync();
        Thread.Sleep(3000);
    }

    private Task LogAsync(LogMessage msg)
    {
        // Log the messages to a file.
        var sw = new StreamWriter(@"data\debug.log", true);
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

        if (args[0] != Prefix)
            return;

        if (msg.Author.IsBot)
            return;

        bool isAdmin = false;
        if (msg.Author.Id == _adminID)
            isAdmin = true;

        await CommandHandler.HandleCommandAsync(msg, args, argsLen, isAdmin);
    }
}
