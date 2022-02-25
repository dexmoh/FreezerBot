using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextGenerator;

public class TextBot
{
    public string FilePath { get; set; }
    public string WordsFilePath { get; set; }
    public Dictionary<string, List<Link>> wordElements { get; set; }

    public TextBot(string filePath, string wordsFilePath)
    {
        FilePath = filePath;
        WordsFilePath = wordsFilePath;
        wordElements = new Dictionary<string, List<Link>>();

        LoadElements();
    }

    public string GenerateLine(int maxWords = 100)
    {
        Random rand = new Random();

        int counter = 0;
        string output = "";
        string currentWord = "/START-OF-SENTENCE/";
        string nextWord = "";

        // Bot has 25% chance to say a 1-word line.
        double randDouble = rand.NextDouble();
        if (randDouble < 0.25)
        {
            var words = File.ReadAllLines(WordsFilePath);
            int randWord = rand.Next(words.Count());

            return words.ElementAt(randWord);
        }

        while (counter < maxWords)
        {
            // Get total link count.
            uint totalLinkCount = Link.GetTotalCount(wordElements[currentWord]);

            // Generate a random double between 0.0 and totalLinkCount.
            int randInt = rand.Next((int)totalLinkCount);
            randDouble = rand.NextDouble();
            randDouble += Convert.ToDouble(randInt);

            // Pick the next word.
            totalLinkCount = 0;
            foreach (Link link in wordElements[currentWord])
            {
                totalLinkCount += link.Count;
                if (randDouble < Convert.ToDouble(totalLinkCount))
                {
                    nextWord = link.Word;
                    break;
                }
            }

            if (nextWord == "/END-OF-SENTENCE/")
                break;

            output += nextWord + " ";
            counter++;
            currentWord = nextWord;
        }

        return output;
    }

    private void LoadElements()
    {
        foreach (string line in File.ReadLines(FilePath))
        {
            string[] args = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < args.Length; i++)
                args[i] = args[i].Replace('`', ' ');

            string word = args[0];

            // Compile list of links.
            List<Link> links = new List<Link>();

            for (int i = 1; i < args.Length; i++)
            {
                // Find the last plus in char array.
                int plusIndex = args[i].Length - 2;
                for (int j = args[i].Length - 1; j > 0; j--)
                {
                    if (args[i][j] == '+')
                    {
                        plusIndex = j;
                        break;
                    }
                }

                // Separate word from the count.
                uint linkCount = UInt32.Parse(args[i].Substring(plusIndex + 1));
                string linkWord = args[i].Substring(0, plusIndex);

                // Add link.
                links.Add(new Link(linkWord, linkCount));
            }

            // Add word.
            wordElements.Add(word, links);
        }
    }
}
