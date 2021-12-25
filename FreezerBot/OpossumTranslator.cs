using System;

namespace FreezerBot
{
    public static class OpossumTranslator
    {
        public static string Translate(string[] strings, int startingElement = 0)
        {
            string translatedStr = "";
            Random rand = new Random(69); // haha funny number

            for (int i = startingElement; i < strings.Length; i++)
            {
                int randNum = rand.Next(3);
                int strLen = strings[i].Length;

                if (randNum == 0)
                {
                    string wehChr;
                    if (rand.Next(2) == 0)
                        wehChr = "E";
                    else
                        wehChr = "A";

                    for (int j = 0; j < strLen; j++)
                    {
                        // Keep special characters
                        if (strings[i][j] > 32 && strings[i][j] < 65)
                        {
                            translatedStr += strings[i][j];
                            continue;
                        }

                        if (j == 0 && strLen > 2)
                            translatedStr += "W";
                        else if (j == strLen - 1 && strLen > 2)
                            translatedStr += "H";
                        else
                            translatedStr += wehChr;
                    }
                }
                else if (randNum == 1)
                {
                    for (int k = 0; k < strLen; k++)
                    {
                        // Keep special characters
                        if (strings[i][k] > 32 && strings[i][k] < 65)
                        {
                            translatedStr += strings[i][k];
                            continue;
                        }

                        if (rand.Next(5) == 0)
                            translatedStr += "a";
                        else
                            translatedStr += "A";
                    }
                }
                else
                {
                    for (int l = 0; l < strLen; l++)
                    {
                        // Keep special characters
                        if (strings[i][l] > 32 && strings[i][l] < 65)
                        {
                            translatedStr += strings[i][l];
                            continue;
                        }

                        if (rand.Next(4) == 0)
                            translatedStr += "h";
                        else
                            translatedStr += "H";
                    }
                }

                translatedStr += " ";
            }

            return translatedStr;
        }
    }
}
