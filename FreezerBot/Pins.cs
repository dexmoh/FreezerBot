using System.Collections.Generic;
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

        // Check if the keyword is too long.
        if (keyword.Length > 40)
        {
            await msg.Channel.SendMessageAsync("The keyword cant be more than 40 characters long.");
            return;
        }

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

    public static async Task ListAsync(SocketMessage msg, int page = 0, int pageSize = 10)
    {
        // Get keywords from a pins.txt file and store them in keywordList.
        string pinsPath = GetPinsPath(msg);
        int counter = 0;
        var keywordList = new List<string>();

        foreach (string line in File.ReadLines(pinsPath))
        {
            if (counter % 2 == 0)
                keywordList.Add($"- {line}\n");

            counter++;
        }

        if (keywordList.Count == 0)
        {
            await msg.Channel.SendMessageAsync($"No saved pins found. Add some by using '{Program.Prefix} pin <keyword>' command!");
            return;
        }

        // Get string of keywords to display.
        string keywords = string.Empty;

        for (int i = page * pageSize; (i < keywordList.Count) && (i < page * pageSize + pageSize); i++)
            keywords += keywordList[i];

        if (keywords == string.Empty)
        {
            await msg.Channel.SendMessageAsync($"Couldn't find any saved pins on page {page + 1}.");
            return;
        }

        // Build the embed.
        var listField = new EmbedFieldBuilder()
            .WithName("Here's a list of all the keywords!")
            .WithValue(keywords)
            .WithIsInline(true);
        var listFooter = new EmbedFooterBuilder()
            .WithText($"Page {page + 1}/{keywordList.Count / pageSize + 1}");
        var listEmbed = new EmbedBuilder()
            .AddField(listField)
            .WithFooter(listFooter)
            .WithColor(Color.Teal)
            .Build();

        await msg.Channel.SendMessageAsync(null, false, listEmbed);
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
