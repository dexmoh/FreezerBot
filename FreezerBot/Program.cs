using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TextGenerator;

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

    // For random text generation.
    public static TextBot TextGenerator;

    Program()
    {
        IsRunning = true;

        // Set bot's prefix.
        Prefix = "poss";

        if (File.Exists("data/prefix.txt"))
            Prefix = File.ReadAllText("data/prefix.txt");
        else
            Console.WriteLine("Couldn't find a 'prefix.txt' file inside of data directory. Setting the bot's prefix to 'poss'.");

        // Set admin ID.
        _adminID = 0;

        if (!File.Exists("data/master.txt"))
            Console.WriteLine("Couldn't find a 'master.txt' file inside of data directory. Admin ID isn't set.");
        else if (!UInt64.TryParse(File.ReadAllText("data/master.txt"), out ulong adminID))
            Console.WriteLine("Couldn't parse admin ID inside of 'master.txt' file. Admin ID isn't set.");
        else
            _adminID = adminID;

        // Text generator stuff.
        TextGenerator = new TextBot("data/FreezerDataOutput.data", "data/FreezerDataParsedWords.txt");

        // Set client and events.
        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += MessageReceivedAsync;

        _client.SetGameAsync("AAAAAAAA", null, ActivityType.Listening);
    }

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        if (!File.Exists("data/token.txt"))
        {
            Console.WriteLine("Couldn't find a 'token.txt' file inside of data directory. " +
                "Make sure /data/token.txt exists and then paste your bot's token value into the txt file.");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, File.ReadLines("data/token.txt").First());
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
        var sw = new StreamWriter("data/debug.log", true);
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

        // Chilly has 2% chance to reply to a random message.
        var rand = new Random();
        double randDouble = rand.NextDouble();
        if (randDouble < 0.02)
        {
            await msg.Channel.SendMessageAsync(TextGenerator.GenerateLine());
        }

        if (msg.Author.IsBot)
            return;

        // Make Chilly reply to mentions.
        bool mentionsBot = false;
        foreach (SocketUser user in msg.MentionedUsers)
        {
            if (user.Id == 922823893125845032)
            {
                mentionsBot = true;
                break;
            }
        }

        if (mentionsBot)
        {
            MessageReference messageReference = new MessageReference(msg.Id, msg.Channel.Id);
            await msg.Channel.SendMessageAsync(TextGenerator.GenerateLine(), false, null, null, null, messageReference);

            return;
        }

        if (args[0] != Prefix)
            return;

        bool isAdmin = false;
        if (msg.Author.Id == _adminID)
            isAdmin = true;

        await CommandHandler.HandleCommandAsync(msg, args, argsLen, isAdmin);
    }
}
