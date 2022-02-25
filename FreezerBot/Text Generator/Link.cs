using System.Collections.Generic;

namespace TextGenerator;

public class Link
{
    public string Word { get; set; }
    public uint Count { get; set; }

    public Link(string word, uint count = 1)
    {
        Word = word;
        Count = count;
    }

    public static uint GetTotalCount(List<Link> links)
    {
        uint result = 0;
        foreach (Link link in links)
        {
            result += link.Count;
        }

        return result;
    }

    public override string ToString()
    {
        return Word + "+" + Count;
    }
}
