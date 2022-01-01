using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FreezerBot;

public static class Pins
{
    public static async Task PinAsync(SocketMessage msg, string[] args, int argsLen)
    {
        string keyword = GetKeyword(msg, args, argsLen);
        string pinsPath = GetPinsPath(msg);

        // Check if the keyword already exists.
        int counter = 0;

        foreach (string line in File.ReadLines(pinsPath))
        {
            if ((counter % 2 == 0) && line == keyword)
            {
                await msg.Channel.SendMessageAsync("That keyword already exists.");
                return;
            }

            counter++;
        }

        // Check if message is a reply.
        MessageReference replyMsgRef = msg.Reference;
        if (replyMsgRef == null)
        {
            await msg.Channel.SendMessageAsync("The message isnt a reply.");
            return;
        }

        // The message the user is replying to.
        IMessage replyMsg = msg.Channel.GetMessageAsync(replyMsgRef.MessageId.Value).Result;

        // The string we'll store the URLs in.
        string urls = string.Empty;

        // Get embed URLs.
        foreach (Embed replyEmbed in replyMsg.Embeds)
            urls += replyEmbed.Url + " ";

        // Get attachment URLs.
        foreach (Attachment attachment in replyMsg.Attachments)
            urls += attachment.Url + " ";

        // Check if the string is empty.
        if (urls.Trim() == string.Empty)
        {
            await msg.Channel.SendMessageAsync("The message has no embeds.");
            return;
        }

        // Write the keyword and the urls to a text file.
        var sw = new StreamWriter(pinsPath, true);
        sw.WriteLine(keyword);
        sw.WriteLine(urls);
        sw.Close();

        await msg.Channel.SendMessageAsync($"Pinned! Use: '{Program.Prefix} lookup {keyword}' to look up the pinned files.");
    }

    public static async Task LookupAsync(SocketMessage msg, string[] args, int argsLen)
    {
        string keyword = GetKeyword(msg, args, argsLen);
        string pinsPath = GetPinsPath(msg);

        // Check if the keyword exists.
        int counter = 0;
        bool keywordExists = false;

        foreach (string line in File.ReadLines(pinsPath))
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
        string urls = File.ReadLines(pinsPath).ElementAtOrDefault(counter + 1);

        await msg.Channel.SendMessageAsync(keyword);
        await msg.Channel.SendMessageAsync(urls);
    }

    public static async Task DeleteAsync(SocketMessage msg, string[] args, int argsLen)
    {
        string keyword = GetKeyword(msg, args, argsLen);
        string pinsPath = GetPinsPath(msg);

        // Check if the keyword exists and store the pins file to a string.
        int counter = 0;
        int keywordLine = -1;
        string pins = string.Empty;

        foreach (string line in File.ReadLines(pinsPath))
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

        File.WriteAllText(pinsPath, pins);
        await msg.Channel.SendMessageAsync($"Successfully deleted \"{keyword}\".");
    }

    public static async Task ListAsync(SocketMessage msg)
    {
        string pinsPath = GetPinsPath(msg);
        int counter = 0;
        string list = string.Empty;

        foreach (string line in File.ReadLines(pinsPath))
        {
            if (counter % 2 == 0)
                list += line + "\n";

            counter++;
        }

        if (list == string.Empty)
        {
            await msg.Channel.SendMessageAsync($"No saved pins found. Add some by using '{Program.Prefix} pin <keyword>' command!");
            return;
        }

        await msg.Channel.SendMessageAsync("**List of all the pinned files:**\n" + list);
    }

    private static string GetKeyword(SocketMessage msg, string[] args, int argsLen)
    {
        // Any string after the 2nd element of args array is considered part of the keyword.
        string keyword = string.Empty;
        for (int i = 2; i < argsLen; i++)
            keyword += args[i] + " ";

        keyword = keyword.Remove(keyword.Length - 1);

        return keyword;
    }

    private static string GetPinsPath(SocketMessage msg)
    {
        // Check if the server directory exists. Create a new one if not.
        SocketGuildChannel channel = msg.Channel as SocketGuildChannel;
        SocketGuild guild = channel.Guild;

        string serverPath = @$"data\servers\{guild.Id}";
        string pinsPath = $@"{serverPath}\pins.txt";

        if (!Directory.Exists(serverPath))
        {
            Directory.CreateDirectory(serverPath);
            File.Create(pinsPath).Close();
        }

        return pinsPath;
    }
}
