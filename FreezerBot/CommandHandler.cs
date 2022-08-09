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
                var helpAuthor = new EmbedAuthorBuilder()
                    .WithName("Help menu!")
                    .WithIconUrl("https://imgur.com/dRLQcoP.png");
                var helpFooter = new EmbedFooterBuilder()
                    .WithText($"Requested by {msg.Author.Username}.")
                    .WithIconUrl(msg.Author.GetAvatarUrl());
                var helpField1 = new EmbedFieldBuilder()
                    .WithName("Info")
                    .WithValue($"Prefix your messages with `{Program.Prefix}` to access bot's commands.\n" +
                    $" Use `{Program.Prefix} <command> help` to get more information regarding a specific command.\n" +
                    $"You can suggest new features and report bugs on our GitHub page. Use `{Program.Prefix} about` command to find out more.")
                    .WithIsInline(true);
                var helpField2 = new EmbedFieldBuilder()
                    .WithName("Basic Commands")
                    .WithValue("`help/commands` - Show help menu.\n" +
                    "`about` - Show about page.\n" +
                    "`translate/say` - Translate human into opossum tongue!\n" +
                    "`show` - Search for an image online.\n" +
                    "`facts` - Tell a random opossum fact.\n");
                var helpField3 = new EmbedFieldBuilder()
                    .WithName("Pins")
                    .WithValue("The bot's main feature is the ability to save embeds (images, gifs, videos) from a message and store them under a keyword. " +
                    "You can then use that keyword to preview the saved embeds.\n\n" +
                    $"Reply to a message with `{Program.Prefix} pin <keyword>` to save its embeds.\n" +
                    $"Use `{Program.Prefix} lookup <keyword>` to lookup saved embeds.\n" +
                    $"Use `{Program.Prefix} delete <keyword>` to delete saved embeds.\n" +
                    $"Use `{Program.Prefix} list` to show a list of all saved embeds.");
                var helpEmbed = new EmbedBuilder()
                    .AddField(helpField1)
                    .AddField(helpField2)
                    .AddField(helpField3)
                    .WithAuthor(helpAuthor)
                    .WithFooter(helpFooter)
                    .WithColor(Color.Teal)
                    .Build();

                await msg.Channel.SendMessageAsync(null, false, helpEmbed);
                break;
            case "about":
                var aboutAuthor = new EmbedAuthorBuilder()
                    .WithName("About me!")
                    .WithIconUrl("https://imgur.com/dRLQcoP.png");
                var aboutFooter = new EmbedFooterBuilder()
                    .WithText($"Requested by {msg.Author.Username}.")
                    .WithIconUrl(msg.Author.GetAvatarUrl());
                var aboutField1 = new EmbedFieldBuilder()
                    .WithName("Intro")
                    .WithValue("Hi, my name is Chilly, I'm an opossum! I live in the Freezer, it's quite cold in here! " +
                    "I'm still in very early development and some features might not work as intended, so cold tight! " +
                    "Currently being hosted in YOUR WALLS. Built with Discord.Net (.NET 6.0).")
                    .WithIsInline(true);
                var aboutField2 = new EmbedFieldBuilder()
                    .WithName("GitHub")
                    .WithValue("Check out my GitHub page by clicking [here](https://github.com/Ewwmewgewd/FreezerBot).\n" +
                    "You can suggest new features, changes and report bugs by opening a new issue, or you can contribute by opening a pull request!");
                var aboutEmbed = new EmbedBuilder()
                    .AddField(aboutField1)
                    .AddField(aboutField2)
                    .WithAuthor(aboutAuthor)
                    .WithFooter(aboutFooter)
                    .WithColor(Color.Teal)
                    .Build();

                await msg.Channel.SendMessageAsync(null, false, aboutEmbed);
                break;
            case "translate":
            case "say":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.Prefix} translate/say <message>\n...\nTranslate human into opossum tongue!```");
                    break;
                }

                await msg.Channel.SendMessageAsync(OpossumTranslator.Translate(args, 2));
                break;
            case "show":
                if (argsLen != 3)
                {
                    string searchTerms = string.Join(", ", Program.ImageSearchWhitelist.Keys.ToArray());

                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.Prefix} show <search term>" +
                        $"\n..." +
                        $"\nSearch for an image online!" +
                        $"\nPossible search terms: " + searchTerms +
                        $"```");
                    break;
                }

                if (Program.ImageSearch == null)
                {
                    await msg.Channel.SendMessageAsync("Image search is unavailable at the moment. :(");
                    break;
                }

                string query = args[2];
                if (!Program.ImageSearchWhitelist.ContainsKey(query))
                {
                    await msg.Channel.SendMessageAsync("Please enter a correct search term. " +
                        "Use `poss show` command to get a list of available search terms.");
                    break;
                }

                string link = await Program.ImageSearch.GallerySearchAsync(Program.ImageSearchWhitelist[query]);
                if (link == "")
                {
                    await msg.Channel.SendMessageAsync("Couldn't find anything under that search term.");
                    break;
                }
                
                await msg.Channel.SendMessageAsync(link);
                break;
            case "facts":
                if ((argsLen == 3) && (args[2] == "help"))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.Prefix} facts\n...\nSend a random opossum fact!```");
                    break;
                }

                var randFact = new Random();
                int randomFact = randFact.Next(53);

                // Build the embed.
                var factField1 = new EmbedFieldBuilder()
                    .WithName("COOL OPOSSUM FACT!")
                    .WithValue(File.ReadLines(@"data/facts.txt").ElementAtOrDefault(randomFact))
                    .WithIsInline(true);
                var factEmbed = new EmbedBuilder()
                    .AddField(factField1)
                    .WithColor(Color.Teal)
                    .Build();

                await msg.Channel.SendMessageAsync(null, false, factEmbed);
                break;
            case "pin":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.Prefix} pin <keyword>\n...\n" +
                        $"Reply to a message with this command to save its embeds. Use '{Program.Prefix} lookup <keyword>' to look up saved files.```");
                    break;
                }

                await Pins.PinAsync(msg, args, argsLen);
                break;
            case "lookup":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.Prefix} lookup <keyword>\n...\nLook up saved files.```");
                    break;
                }

                await Pins.LookupAsync(msg, args, argsLen);
                break;
            case "delete":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.Prefix} delete <keyword>\n...\nDelete saved embeds.```");
                    break;
                }

                await Pins.DeleteAsync(msg, args, argsLen);
                break;
            case "list":
                if (argsLen == 2)
                    await Pins.ListAsync(msg);
                else if (argsLen == 3)
                {
                    if (args[2] == "help")
                    {
                        await msg.Channel.SendMessageAsync($"```Command usage: {Program.Prefix} list <page number>\n...\nShow a list of all saved embeds.```");
                        break;
                    }

                    if (!Int32.TryParse(args[2], out int page))
                    {
                        await msg.Channel.SendMessageAsync("Third argument has to be a number.");
                        break;
                    }

                    if (page < 1)
                    {
                        await msg.Channel.SendMessageAsync("Page number cant be less than 1.");
                        break;
                    }

                    await Pins.ListAsync(msg, page - 1);
                }

                break;
            case "shutdown":
                if ((argsLen == 3) && (args[2] == "help"))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.Prefix} shutdown\n...\nShut down the bot. Only bot author can call this command.```");
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
                Program.IsRunning = false;
                break;
            default:
                break;
        }
    }
}
