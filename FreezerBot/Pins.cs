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

        await msg.Channel.SendMessageAsync($"Pinned! Use: `{Program.prefix} lookup {keyword}` to look up the pinned files.");
    }

    public static async Task LookupAsync(SocketMessage msg, string[] args, int argsLen)
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

    public static async Task DeleteAsync(SocketMessage msg, string[] args, int argsLen)
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
