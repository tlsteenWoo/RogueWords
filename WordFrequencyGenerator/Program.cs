using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WordFrequencyGenerator
{
    class Program
    {
        static int[] common = { 1, 2, 5, 10, 15 };
        static Dictionary<int, List<string>> commonWords = new Dictionary<int, List<string>>();
        static bool resetFrequency = false;
        static int pageCount = 0;
        static int maxFrequency = 0;
        static double elapsed;
        static string directory = "C:/Users/tlawr/Documents/GitHub/RogueWords/RogueWordsDesktop/bin/Debug Desktop/RogueWords/";
        static Dictionary<char, Dictionary<int, Dictionary<string,int>>> wordTable = new Dictionary<char, Dictionary<int, Dictionary<string,int>>>();

        static double pagesPerSecond
        {
            get { return elapsed / (double)pageCount; }
        }

        static string GetFileName(int letter)
        {
            return directory + "Clean " + (char)(65 + letter) + " Words.txt";
        }
        static void EvaluateCommon(string word, int frequency)
        {
            for (int i = 0; i < common.Length; ++i)
            {
                if (!commonWords.ContainsKey(i))
                    commonWords.Add(i, new List<string>());
                if (frequency < common[i])
                {
                    commonWords[i].Add(word);
                    break;
                }
            }
        }
        static void LoadWords()
        {
            for(int i = 0; i < 26; ++i)
            {
                string fileName = GetFileName(i);
                string contents = string.Empty;
                using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
                {
                    contents = reader.ReadToEnd();
                    }
                var lines = contents.Split(new string[] { "\r\n" },StringSplitOptions.None);
                for(int j = 0; j < lines.Length;++j)
                {
                    var tokens = lines[j].Split(',');
                    var word = tokens[0];
                    if (string.IsNullOrWhiteSpace(word)) continue;
                    var frequency = 0;
                    if(!resetFrequency && tokens.Length > 1)
                        frequency = int.Parse(tokens[1]);
                    maxFrequency = Math.Max(frequency, maxFrequency);
                    if (!wordTable.ContainsKey(word[0]))
                        wordTable.Add(word[0], new Dictionary<int, Dictionary<string, int>>());
                    if (!wordTable[word[0]].ContainsKey(word.Length))
                        wordTable[word[0]].Add(word.Length, new Dictionary<string, int>());
                    wordTable[word[0]][word.Length].Add(word, frequency);
                    EvaluateCommon(word, frequency);
                }
            }
        }
        static void WriteWords()
        {
            for(int i = 0; i < 26; ++i)
            {
                string fileName = GetFileName(i);
                using (StreamWriter writer = new StreamWriter(File.Create(fileName)))
                {
                    var chars = wordTable[(char)(65 + i)];
                    foreach(var length in chars.Keys)
                    {
                        var strings = chars[length];
                        foreach(var stringInt in strings)
                        {
                            writer.WriteLine(stringInt.Key + "," + stringInt.Value);
                        }
                    }
                }
            }
        }
        static bool AddWordInstance(string inputWord)
        {
            var word = inputWord.ToUpper();
            if (string.IsNullOrWhiteSpace(word))
                return false;
            if (!wordTable.ContainsKey(word[0]))
                return false;
            if (!wordTable[word[0]].ContainsKey(word.Length))
                return false;
            var wordCount = wordTable[word[0]][word.Length];
            if (!wordCount.ContainsKey(word))
                return false;
            int frequency = wordCount[word] + 1;
            wordCount[word] = frequency;
            if(frequency > maxFrequency)
            {
                maxFrequency = frequency;
                Console.WriteLine("Max Frequency:" + maxFrequency);
            }
            return true;
        }
        static void LoadRandomPage()
        {
            WebRequest request = WebRequest.Create("https://en.wikipedia.org/wiki/Special:Random");
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            Console.WriteLine("Web Request: " + response.ResponseUri);
            string html = String.Empty;
            using (StreamReader sr = new StreamReader(data))
            {
                html = sr.ReadToEnd();
            }
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var nodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'mw-parser-output')]");
            if (nodes.Count > 1 || nodes.Count == 0)
                Debugger.Break();
            var children = nodes[0].ChildNodes;
            string text = string.Empty;
            foreach (var c in children)
            {
                if (c.Name == "p")
                {
                    text += c.InnerText;
                }
            }
            var words = text.Split(' ', '\n', '\r');
            foreach (var word in words)
            {
                AddWordInstance(word);
            }
            pageCount++;
        }
        static void Main(string[] args)
        {
            LoadWords();
            Console.WriteLine("Max Frequency: " + maxFrequency);
            Thread thread = new Thread(() =>
            {
                Stopwatch watch = new Stopwatch();
                while (true)
                {
                    watch.Start();
                    LoadRandomPage();
                    watch.Stop();
                    watch.Reset();
                    elapsed += watch.Elapsed.TotalSeconds;
                }
            });
            thread.Start();
            var key = Console.ReadKey();
            thread.Abort();
            WriteWords();
            int n = 0;
        }
    }
}
