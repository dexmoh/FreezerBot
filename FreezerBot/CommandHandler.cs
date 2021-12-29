using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                await msg.Channel.SendMessageAsync(System.IO.File.ReadAllText(@"data\about.txt"));
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

                await Pins.Pin(msg, args, argsLen);
                break;
            case "lookup":
                if (argsLen == 2)
                {
                    int counter = 0;
                    string list = "";
                    foreach (string line in File.ReadLines(@"data\pins.txt"))
                    {
                        if (counter % 2 == 0)
                            list += line + "\n";

                        counter++;
                    }

                    if (list == string.Empty)
                    {
                        await msg.Channel.SendMessageAsync("No saved pins found. Add some by using \"poss pin <keyword>\" command!");
                        break;
                    }

                    await msg.Channel.SendMessageAsync("**List of all the pinned files:**\n" + list);
                    break;
                }
                else if ((argsLen == 3) && (args[2] == "help"))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} lookup <keyword>\n...\nLook up saved files.```");
                    break;
                }

                await Pins.Lookup(msg, args, argsLen);
                break;
            case "delete":
                if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                {
                    await msg.Channel.SendMessageAsync($"```Command usage: {Program.prefix} delete <keyword>\n...\nDelete saved embeds.```");
                    break;
                }

                await Pins.Delete(msg, args, argsLen);
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
