using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MknGames.Rogue_Words
{
    public static class WordListProcessor
    {
        public static Dictionary<char, Dictionary<int, List<string>>> words =
            new Dictionary<char, Dictionary<int, List<string>>>();
        public static void DoWorkSon()
        {
            using (StreamReader reader = new StreamReader(File.OpenRead("Content/words_alpha.txt")))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string word = line.Trim();
                    if (string.IsNullOrEmpty(word))
                        continue;
                    char letter = word[0];
                    if (!words.ContainsKey(letter))
                    {
                        words.Add(letter, new Dictionary<int, List<string>>());
                    }
                    Dictionary<int, List<string>> table = words[letter];
                    int count = word.Length;
                    if (!table.ContainsKey(count))
                    {
                        table.Add(count, new List<string>());
                    }
                    List<string> wordList = table[count];
                    if (!wordList.Contains(word))
                    {
                        wordList.Add(word);
                    }
                }
            }
            
            foreach (char letter in words.Keys)
            {
                foreach (int count in words[letter].Keys)
                {
                    using (StreamWriter writer = new StreamWriter(File.Create("Rogue Words/Processed Wordlist/" + letter + "-" + count + ".txt")))
                    {
                        foreach (string word in words[letter][count])
                        {
                            writer.Write(word + '\n');
                        }
                    }
                }
            }
        }
    }
}