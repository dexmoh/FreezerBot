using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.WebSocket;

namespace Nationstates;

public class NationstatesClient
{
    private HttpClient _httpClient;
    private Nation _nation;
    private IssueAnswer _iAnswer;
    private List<Issue> _issues;
    private string _uri;
    private string _nationName;
    private string _nationNameFull;
    private string _nationPass;
    private string _sessionPin;

    public NationstatesClient(string nationName, string nationPass, string userAgent)
    {
        _uri = "https://www.nationstates.net/cgi-bin/api.cgi";
        _nationName = nationName.ToLower().Replace(' ', '_');
        _nationNameFull = nationName;
        _nationPass = nationPass;
        _sessionPin = string.Empty;

        // Set up HttpClient.
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        _httpClient.DefaultRequestHeaders.Add("X-Password", _nationPass);
        _httpClient.DefaultRequestHeaders.Add("X-Pin", "0");

        _nation = new Nation();
        _iAnswer = new IssueAnswer();
        _issues = new List<Issue>();
    }

    public async Task HandleCommandAsync(SocketMessage msg, string[] args, int argsLen)
    {
        await LoginAsync();

        // nationstates help
        if (argsLen == 2 && args[1] == "help")
        {
            var author = new EmbedAuthorBuilder()
                .WithName("Nationstates help menu!")
                .WithIconUrl("https://imgur.com/dRLQcoP.png");
            var footer = new EmbedFooterBuilder()
                .WithText($"Requested by {msg.Author.Username}.")
                .WithIconUrl(msg.Author.GetAvatarUrl());
            var embed = new EmbedBuilder()
                .WithAuthor(author)
                .WithFooter(footer)
                .WithColor(Color.Teal);

            embed.AddField(new EmbedFieldBuilder().WithName("Info").WithValue("Prefix your messages with `nationstates` to access nationstates commands."));
            embed.AddField(new EmbedFieldBuilder().WithName("Commands").WithValue(
                "`info` - Display info about the nation.\n" +
                "`list issues` - List all current issues.\n" +
                "`get issue <id>` - Show info about a specific issue.\n" +
                "`answer issue <issue id> <option id>` - Answer an issue."));
            embed.AddField(new EmbedFieldBuilder().WithName("Disclaimer").WithValue("I just put this thing together in 2 days and it's probably full of bugs that I'll deal with eventually."));

            await msg.Channel.SendMessageAsync(null, false, embed.Build());
            return;
        }

        // nationstates info
        if (argsLen == 2 && args[1] == "info")
        {
            await FetchNationInfoAsync();

            var author = new EmbedAuthorBuilder()
                .WithName(_nation.Fullname)
                .WithIconUrl("https://imgur.com/dRLQcoP.png");
            var footer = new EmbedFooterBuilder()
                .WithText($"Requested by {msg.Author.Username}.")
                .WithIconUrl(msg.Author.GetAvatarUrl());
            var embed = new EmbedBuilder()
                .WithAuthor(author)
                .WithFooter(footer)
                .WithColor(Color.Teal);


            embed.AddField(new EmbedFieldBuilder().WithName("Civil Rights").WithValue(_nation.CivilRights).WithIsInline(true));
            embed.AddField(new EmbedFieldBuilder().WithName("Economy").WithValue(_nation.Economy).WithIsInline(true));
            embed.AddField(new EmbedFieldBuilder().WithName("Political Freedom").WithValue(_nation.PoliticalFreedom).WithIsInline(true));

            embed.AddField(new EmbedFieldBuilder().WithName(_nation.Category).WithValue($"\"{_nation.Motto}\""));
            
            string info =
                "Population\n" +
                "Leader\n" +
                "National animal\n" +
                "Capital city\n" +
                "Currency\n" +
                "Major religion\n" +
                "Major industry\n" +
                "Answered issues";

            string desc = string.Empty;
            desc += _nation.Population + " million\n";
            desc += _nation.Leader + "\n";
            desc += _nation.Animal + "\n";
            desc += _nation.Capital + "\n";
            desc += _nation.Currency + "\n";
            desc += _nation.Religion + "\n";
            desc += _nation.MajorIndustry + "\n";
            desc += _nation.AnsweredIssues;

            embed.AddField(new EmbedFieldBuilder().WithName("Info").WithValue(info).WithIsInline(true));
            embed.AddField(new EmbedFieldBuilder().WithName("Description").WithValue(desc).WithIsInline(true));

            await msg.Channel.SendMessageAsync(null, false, embed.Build());
            return;
        }
        
        // nationstates list issues
        if (argsLen == 3 && args[1] == "list" && args[2] == "issues")
        {
            string issueIDs = string.Empty;
            string issueTitles = string.Empty;

            await FetchIssuesAsync();
            foreach (Issue issue in _issues)
            {
                issueIDs += issue.ID + "\n";
                issueTitles += issue.Title + "\n";
            }

            var issuesAuthor = new EmbedAuthorBuilder()
                .WithName($"The following issues confront {_nationNameFull}:")
                .WithIconUrl("https://imgur.com/dRLQcoP.png");
            var issuesFooter = new EmbedFooterBuilder()
                .WithText($"Requested by {msg.Author.Username}.")
                .WithIconUrl(msg.Author.GetAvatarUrl());
            var issuesEmbed = new EmbedBuilder()
                .WithAuthor(issuesAuthor)
                .WithFooter(issuesFooter)
                .WithColor(Color.Teal);

            if (issueIDs == string.Empty)
            {
                string nextIssue = await FetchIssueTimeAsync();
                issuesEmbed.AddField(new EmbedFieldBuilder().WithName($"{_nationNameFull} is gloriously issue-free!").WithValue($"Next issue will be available {nextIssue}."));
                await msg.Channel.SendMessageAsync(null, false, issuesEmbed.Build());
                return;
            }
            
            var issuesField1 = new EmbedFieldBuilder()
                .WithName("ID")
                .WithValue(issueIDs)
                .WithIsInline(true);
            var issuesField2 = new EmbedFieldBuilder()
                .WithName("Title")
                .WithValue(issueTitles)
                .WithIsInline(true);
            issuesEmbed.AddField(issuesField1).AddField(issuesField2);

            await msg.Channel.SendMessageAsync(null, false, issuesEmbed.Build());
            return;
        }
    
        // nationstates get issue <id>
        if (argsLen == 4 && args[1] == "get" && args[2] == "issue")
        {
            string issueID = string.Empty;
            string issueTitle = string.Empty;
            string issueText = string.Empty;
            List<(string ID, string Text)> issueOptions = new();

            await FetchIssuesAsync();
            foreach (Issue issue in _issues)
            {
                if (args[3] == issue.ID)
                {
                    issueID = issue.ID;
                    issueTitle = issue.Title;
                    issueText = issue.Text;
                    issueOptions = issue.Options;
                    break;
                }
            }

            var issuesAuthor = new EmbedAuthorBuilder()
                .WithName($"Issue {args[3]}")
                .WithIconUrl("https://imgur.com/dRLQcoP.png");
            var issuesFooter = new EmbedFooterBuilder()
                .WithText($"Requested by {msg.Author.Username}.")
                .WithIconUrl(msg.Author.GetAvatarUrl());
            var issuesEmbed = new EmbedBuilder()
                .WithAuthor(issuesAuthor)
                .WithFooter(issuesFooter)
                .WithColor(Color.Teal);

            if (issueID == string.Empty)
            {
                issuesEmbed.AddField(new EmbedFieldBuilder()
                    .WithName("Invalid issue")
                    .WithValue("You've provided a wrong issue ID. Use 'nationstates list issues' to get a list of issues and their IDs."));

                await msg.Channel.SendMessageAsync(null, false, issuesEmbed.Build());
                return;
            }

            var issuesFieldTitle = new EmbedFieldBuilder()
                .WithName(issueTitle)
                .WithValue(issueText);
            issuesEmbed.AddField(issuesFieldTitle);

            foreach (var option in issueOptions)
                issuesEmbed.AddField(new EmbedFieldBuilder().WithName($"Option {option.ID}").WithValue(option.Text));

            await msg.Channel.SendMessageAsync(null, false, issuesEmbed.Build());
            return;
        }

        // nationstates answer issue <issue id> <option id>
        if (argsLen == 5 && args[1] == "answer" && args[2] == "issue")
        {
            await AnswerIssueAsync(args[3], args[4]);

            var issuesAuthor = new EmbedAuthorBuilder()
                    .WithName($"Issue {args[3]}")
                    .WithIconUrl("https://imgur.com/dRLQcoP.png");
            var issuesFooter = new EmbedFooterBuilder()
                .WithText($"Requested by {msg.Author.Username}.")
                .WithIconUrl(msg.Author.GetAvatarUrl());
            var issuesEmbed = new EmbedBuilder()
                .WithAuthor(issuesAuthor)
                .WithFooter(issuesFooter)
                .WithColor(Color.Teal);

            if (_iAnswer.IssueID != args[3])
            {
                issuesEmbed.AddField(new EmbedFieldBuilder()
                    .WithName("Invalid issue")
                    .WithValue("You've provided a wrong issue ID or option ID. Use 'nationstates list issues' to get a list of issues and their IDs."));

                await msg.Channel.SendMessageAsync(null, false, issuesEmbed.Build());
                return;
            }

            var issuesFieldTitle = new EmbedFieldBuilder()
                .WithName("Issue answered")
                .WithValue(_iAnswer.Answer);
            issuesEmbed.AddField(issuesFieldTitle);

            await msg.Channel.SendMessageAsync(null, false, issuesEmbed.Build());
            return;
        }
    }

    private async Task<bool> LoginAsync()
    {
        // Send a GET request.
        string uri = GetFullURI("ping");
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
            return false;

        // Get session pin.
        IEnumerable<string> pin;
        if (!response.Headers.TryGetValues("X-Pin", out pin))
            return false;

        if (pin.FirstOrDefault() == null)
            return false;

        // Set session pin.
        _sessionPin = pin.FirstOrDefault();
        _httpClient.DefaultRequestHeaders.Remove("X-Pin");
        _httpClient.DefaultRequestHeaders.Add("X-Pin", _sessionPin);

        return true;
    }

    private async Task<bool> AnswerIssueAsync(string issueID, string optionID)
    {
        string uri = GetFullURI("issue", true);
        uri += $"&issue={issueID}&option={optionID}";
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
            return false;

        File.WriteAllText("DEBUG.xml", await response.Content.ReadAsStringAsync()); // DEBUG delete later

        _iAnswer = new IssueAnswer();
        var doc = new XmlDocument();
        doc.LoadXml(await response.Content.ReadAsStringAsync());

        XmlNode issueNode = doc.GetElementsByTagName("ISSUE").Item(0);
        _iAnswer.IssueID = issueNode.Attributes["id"].Value;
        _iAnswer.OptionID = issueNode.Attributes["choice"].Value;
        _iAnswer.Answer = issueNode.SelectSingleNode("DESC").InnerText;

        return true;
    }

    private async Task<bool> FetchNationInfoAsync()
    {
        string uri = GetFullURI();
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
            return false;

        _nation = new Nation();
        var doc = new XmlDocument();
        doc.LoadXml(await response.Content.ReadAsStringAsync());

        XmlNode nationNode = doc.GetElementsByTagName("NATION").Item(0);
        _nation.Name = nationNode.SelectSingleNode("NAME").InnerText;
        _nation.Fullname = nationNode.SelectSingleNode("FULLNAME").InnerText;
        _nation.Motto = nationNode.SelectSingleNode("MOTTO").InnerText;
        _nation.Category = nationNode.SelectSingleNode("CATEGORY").InnerText;
        _nation.AnsweredIssues = nationNode.SelectSingleNode("ISSUES_ANSWERED").InnerText;
        _nation.Population = nationNode.SelectSingleNode("POPULATION").InnerText;
        _nation.Animal = nationNode.SelectSingleNode("ANIMAL").InnerText;
        _nation.Currency = nationNode.SelectSingleNode("CURRENCY").InnerText;
        _nation.MajorIndustry = nationNode.SelectSingleNode("MAJORINDUSTRY").InnerText;
        _nation.Leader = nationNode.SelectSingleNode("LEADER").InnerText;
        _nation.Capital = nationNode.SelectSingleNode("CAPITAL").InnerText;
        _nation.Religion = nationNode.SelectSingleNode("RELIGION").InnerText;

        XmlNode freedomNode = nationNode.SelectSingleNode("FREEDOM");
        _nation.CivilRights = freedomNode.SelectSingleNode("CIVILRIGHTS").InnerText;
        _nation.Economy = freedomNode.SelectSingleNode("ECONOMY").InnerText;
        _nation.PoliticalFreedom = freedomNode.SelectSingleNode("POLITICALFREEDOM").InnerText;

        return true;
    }

    private async Task<bool> FetchIssuesAsync()
    {
        // Get a list of all active issues from the server and store them in an Issue list.
        string uri = GetFullURI("issues");
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
            return false;

        _issues = new List<Issue>();
        var doc = new XmlDocument();
        doc.LoadXml(await response.Content.ReadAsStringAsync());
        foreach (XmlNode node in doc.GetElementsByTagName("ISSUE"))
        {
            var issue = new Issue();
            issue.ID = node.Attributes["id"].Value;
            issue.Title = node.SelectSingleNode("TITLE").InnerText;
            issue.Text = node.SelectSingleNode("TEXT").InnerText;

            issue.Options = new List<(string ID, string Text)>();
            foreach (XmlNode child in node.SelectNodes("OPTION"))
                issue.Options.Add((child.Attributes["id"].Value, child.InnerText));

            _issues.Add(issue);
        }

        return true;
    }

    private async Task<string> FetchIssueTimeAsync()
    {
        string uri = GetFullURI("nextissue");
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var doc = new XmlDocument();
        doc.LoadXml(await response.Content.ReadAsStringAsync());
        return doc.SelectSingleNode("NATION").SelectSingleNode("NEXTISSUE").InnerText;
    }

    private async Task<List<(string ID, string Title)>> GetIssueSummaryAsync()
    {
        var result = new List<(string ID, string Title)>();

        string uri = GetFullURI("issuesummary");
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
            return null;

        var doc = new XmlDocument();
        doc.LoadXml(await response.Content.ReadAsStringAsync());
        foreach (XmlNode node in doc.GetElementsByTagName("ISSUE"))
            result.Add((node.Attributes["id"].Value, node.InnerText));

        return result;
    }

    private string GetFullURI()
    {
        return _uri + "?nation=" + _nationName;
    }

    private string GetFullURI(string shard, bool isCommand = false)
    {
        string result = _uri + "?nation=" + _nationName;

        if (isCommand)
            result += "&c=";
        else
            result += "&q=";

        result += shard;
        return result;
    }

    private string GetFullURI(List<string> shards)
    {
        string result = _uri + "?nation=" + _nationName + "&q=";

        foreach (string shard in shards)
        {
            result += shard + "+";
        }

        return result.Remove(result.Length - 1);
    }
}
