using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FreezerBot
{
    public static class CommandHandler
    {
        private static SocketMessage msg;
        private static string[] args;
        private static int argsLen;
        private static string prefix;

        public static async Task HandleCommandAsync(SocketMessage socketMessage, string[] arguments, int argumentsLen, string botPrefix, bool isMaster)
        {
            msg = socketMessage;
            args = arguments;
            argsLen = argumentsLen;
            prefix = botPrefix;

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
                    await msg.Channel.SendMessageAsync(string.Format(System.IO.File.ReadAllText(@"data\help.txt"), prefix));
                    break;
                case "about":
                    await msg.Channel.SendMessageAsync(System.IO.File.ReadAllText(@"data\about.txt"));
                    break;
                case "translate":
                case "say":
                    if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                    {
                        await msg.Channel.SendMessageAsync($"```Command usage: {prefix} translate/say <message>\n...\nTranslate human into opossum tongue!```");
                        break;
                    }
                    
                    await msg.Channel.SendMessageAsync(OpossumTranslator.Translate(args, 2));
                    break;
                case "show":
                    if (argsLen == 2 || ((argsLen == 3) && (args[2] != "poss")))
                    {
                        await msg.Channel.SendMessageAsync($"```Command usage: {prefix} show poss\n...\nSend a random opossum image!```");
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
                        await msg.Channel.SendMessageAsync($"```Command usage: {prefix} facts\n...\nSend a random opossum fact!```");
                        break;
                    }

                    Random randFact = new Random();
                    int randomFact = randFact.Next(53);
                    string fact = "COOL OPOSSUM FACT:";
                    fact += File.ReadLines(@"data\facts.txt").ElementAtOrDefault(randomFact);

                    await msg.Channel.SendMessageAsync(fact);
                    break;
                case "pin":
                    if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                    {
                        await msg.Channel.SendMessageAsync($"```Command usage: {prefix} pin <keyword>\n...\nReply to a message with this command to save its embeds. Use \"{prefix} lookup <keyword>\" to look up saved files.```");
                        break;
                    }

                    await PinComAsync();
                    break;
                case "lookup":
                    if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                    {
                        await msg.Channel.SendMessageAsync($"```Command usage: {prefix} lookup <keyword>\n...\nLook up saved files.```");
                        break;
                    }

                    await LookupComAsync();
                    break;
                case "delete":
                    if (argsLen == 2 || ((argsLen == 3) && (args[2] == "help")))
                    {
                        await msg.Channel.SendMessageAsync($"```Command usage: {prefix} delete <keyword>\n...\nDelete saved embeds.```");
                        break;
                    }

                    await DeleteComAsync();
                    break;
                case "shutdown":
                    if ((argsLen == 3) && (args[2] == "help"))
                    {
                        await msg.Channel.SendMessageAsync($"```Command usage: {prefix} shutdown\n...\nShut down the bot. Only bot author can call this command.```");
                        break;
                    }

                    if (!isMaster)
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

        private static async Task PinComAsync()
        {
            // Fetch keyword.
            string keyword = "";
            for (int i = 2; i < argsLen; i++)
                keyword += args[i] + " ";

            // Check if keyword string is empty.
            if (keyword == string.Empty)
            {
                await msg.Channel.SendMessageAsync("You have to provide a keyword.");
                return;
            }

            // Remove the space at the end. 
            keyword = keyword.Remove(keyword.Length - 1);

            // Check if the keyword already exists.
            int counter = 0;
            bool keywordExists = false;

            foreach (string line in File.ReadLines(@"data\pins.txt"))
            {
                if ((counter % 2 == 0) && line == keyword)
                {
                    await msg.Channel.SendMessageAsync("That keyword already exists.");
                    keywordExists = true;
                    break;
                }

                counter++;
            }

            if (keywordExists)
                return;

            // Check if message is a reply.
            var replyMsgRef = msg.Reference;
            if (replyMsgRef == null)
            {
                await msg.Channel.SendMessageAsync("The message isnt a reply.");
                return;
            }

            // The message the user is replying to.
            IMessage replyMsg = msg.Channel.GetMessageAsync(replyMsgRef.MessageId.Value).Result;

            // The string we'll store the URLs in.
            string urls = "";

            // Get embed URLs.
            foreach (Embed replyEmbed in replyMsg.Embeds)
                urls += replyEmbed.Url + " ";

            // Get attachment URLs.
            foreach (Attachment attachment in replyMsg.Attachments)
                urls += attachment.Url + " ";

            // Check if the string is empty.
            if (urls == string.Empty)
            {
                await msg.Channel.SendMessageAsync("The message has no embeds.");
                return;
            }

            // Write the keyword and the urls to a text file.
            StreamWriter sw = new StreamWriter(@"data\pins.txt", true);
            sw.WriteLine(keyword);
            sw.WriteLine(urls);
            sw.Close();

            await msg.Channel.SendMessageAsync($"Pinned! Use: `{prefix} lookup {keyword}` to look up the pinned files.");
        }

        private static async Task LookupComAsync()
        {
            // Fetch keyword.
            string keyword = "";
            for (int i = 2; i < argsLen; i++)
                keyword += args[i] + " ";

            // Check if keyword string is empty.
            if (keyword == string.Empty)
            {
                await msg.Channel.SendMessageAsync("You have to provide a keyword.");
                return;
            }

            // Remove the space at the end. 
            keyword = keyword.Remove(keyword.Length - 1);

            // Check if the keyword exists.
            int counter = 0;
            bool keywordExists = false;

            foreach (string line in File.ReadLines(@"data\pins.txt"))
            {
                if ((counter % 2 == 0) && line == keyword)
                {
                    keywordExists = true;
                    break;
                }

                counter++;
            }

            if (!keywordExists)
            {
                await msg.Channel.SendMessageAsync("That keyword doesn't exist.");
                return;
            }

            // Fetch the URLs.
            string urls;
            urls = File.ReadLines(@"data\pins.txt").ElementAtOrDefault(counter + 1);

            await msg.Channel.SendMessageAsync(keyword);
            await msg.Channel.SendMessageAsync(urls);
        }

        private static async Task DeleteComAsync()
        {
            // Fetch keyword.
            string keyword = "";
            for (int i = 2; i < argsLen; i++)
                keyword += args[i] + " ";

            // Check if keyword string is empty.
            if (keyword == string.Empty)
            {
                await msg.Channel.SendMessageAsync("You have to provide a keyword.");
                return;
            }

            // Remove the space at the end. 
            keyword = keyword.Remove(keyword.Length - 1);

            // Check if the keyword exists and store the pins file to a string.
            int counter = 0;
            int keywordLine = -1;
            string pins = "";

            foreach (string line in File.ReadLines(@"data\pins.txt"))
            {
                counter++;

                if ((counter % 2 == 1) && line == keyword)
                {
                    keywordLine = counter;
                    continue;
                }
                else if (keywordLine == counter - 1)
                    continue;

                pins += line + "\n";
            }

            if (keywordLine == -1)
            {
                await msg.Channel.SendMessageAsync("That keyword doesn't exist.");
                return;
            }

            File.WriteAllText(@"data\pins.txt", pins);
            await msg.Channel.SendMessageAsync($"Successfully deleted \"{keyword}\".");
        }
    }
}
