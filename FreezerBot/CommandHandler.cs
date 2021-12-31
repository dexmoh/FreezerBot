using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FreezerBot;

public static class CommandHandler
{
    public static async Task HandleCommandAsync(SocketMessage msg, string[] args, int argsLen, bool isAdmin)
    {
        // Ping!
        if (argsLen < 2)
        {
            await msg.Channel.SendMessageAsync("Hi!");
            return;
        }

        // bitch!!!
        if ((argsLen == 2) && (args[1].StartsWith("bitch")))
        {
            await msg.Channel.SendMessageAsync("https://media.discordapp.net/attachments/622200209015046220/831832692835221554/bitch.gif");
            return;
        }

        // Handle commands.
        switch (args[1])
        {
            case "help":
            case "commands":
                await msg.Channel.SendMessageAsync(string.Format(System.IO.File.ReadAllText(@"data\help.txt"), Program.prefix));
                break;
            case "about":
                var embedAuthor = new EmbedAuthorBuilder()
                    .WithName("About me!")
                    .WithIconUrl("https://imgur.com/dRLQcoP.png");
                var embedFooter = new EmbedFooterBuilder()
                    .WithText($"Requested by {msg.Author.Username}.")
                    .WithIconUrl(msg.Author.GetAvatarUrl());
                var embedField1 = new EmbedFieldBuilder()
                    .WithName("Intro")
                    .WithValue("Hi, my name is Chilly, I'm an opossum! I live in the Freezer, it's quite cold in here! " +
                    "I'm still in very early development and some features might not work as intended, so cold tight! " +
                    "Currently being hosted on Microsoft Azure VM. Built with Discord.Net (.NET 6.0).")
                    .WithIsInline(true);
                var embedField2 = new EmbedFieldBuilder()
                    .WithName("GitHub")
                    .WithValue("Check out my GitHub page by clicking [here](https://github.com/Ewwmewgewd/FreezerBot).\n" +
                    "You can suggest new features and changes by opening a new issue, or you can contribute by opening a pull request!");
                var embed = new EmbedBuilder()
                    .AddField(embedField1)
                    .AddField(embedField2)
                    .WithAuthor(embedAuthor)
                    .WithFooter(embedFooter)
                    .WithColor(Color.Teal)
                    .Build();

                await msg.Channel.SendMessageAsync(null, false, embed);
                break;
            case "translate":
            case "say":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} translate/say <message>\n...\nTranslate human into opossum tongue!```");
                    break;
                }

                await msg.Channel.SendMessageAsync(OpossumTranslator.Translate(args, 2));
                break;
            case "show":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] != "poss")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} show poss\n...\nSend a random opossum image!```");
                    break;
                }

                Random randImage = new Random();
                int randomImage = randImage.Next(77);
                string url = File.ReadLines(@"data\cute opossum images.txt").ElementAtOrDefault(randomImage);
                await msg.Channel.SendMessageAsync(url);
                break;
            case "facts":
                if ((argsLen == 3) && (args[2] == "help"))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} facts\n...\nSend a random opossum fact!```");
                    break;
                }

                Random randFact = new Random();
                int randomFact = randFact.Next(53);
                string fact = "COOL OPOSSUM FACT: ";
                fact += File.ReadLines(@"data\facts.txt").ElementAtOrDefault(randomFact);

                await msg.Channel.SendMessageAsync(fact);
                break;
            case "pin":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} pin <keyword>\n...\nReply to a message with this command to save its embeds. Use \"{Program.prefix} lookup <keyword>\" to look up saved files.```");
                    break;
                }

                await Pins.PinAsync(msg, args, argsLen);
                break;
            case "lookup":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} lookup <keyword>\n...\nLook up saved files.```");
                    break;
                }

                await Pins.LookupAsync(msg, args, argsLen);
                break;
            case "delete":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} delete <keyword>\n...\nDelete saved embeds.```");
                    break;
                }

                await Pins.DeleteAsync(msg, args, argsLen);
                break;
            case "list":
                if (argsLen == 2)
                    await Pins.ListAsync(msg, args, argsLen);
                else if ((argsLen == 3) && (args[2] == "help"))
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} list\n...\nShow a list of all saved embeds.```");
                break;
            case "shutdown":
                if ((argsLen == 3) && (args[2] == "help"))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} shutdown\n...\nShut down the bot. Only bot author can call this command.```");
                    break;
                }

                if (!isAdmin)
                {
                    await msg.Channel.SendMessageAsync("HAHAHAHAHAHA!");
                    await msg.Channel.SendMessageAsync("https://tenor.com/view/lotr-lord-of-the-rings-theoden-king-of-rohan-you-have-no-power-here-gif-4952489");
                    break;
                }

                // Exit the program.
                await msg.Channel.SendMessageAsync("Shutting down... :(");
                Program.isRunning = false;
                break;
            default:
                break;
        }
    }
}
