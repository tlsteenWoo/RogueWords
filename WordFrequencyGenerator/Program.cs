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
    class WordData
    {
        public int wikiFrequency;
        public int baiduFrequency;
    }
    class Program
    {
        static int[] common = { 1, 2, 5, 10, 15 };
        static Dictionary<int, List<string>> commonWords = new Dictionary<int, List<string>>();
        static bool resetFrequency = false;
        static bool resetBaidu = false;
        static int pageCount = 0;
        static int maxFrequency = 0;
        static int baiduPageCount = 0;
        static double elapsed;
        static TimeSpan elapsedTimeSpan;
        static string directory = "C:/Users/tlawr/Documents/GitHub/RogueWords/RogueWordsDesktop/bin/Debug Desktop/RogueWords/";
        static Dictionary<char, Dictionary<int, Dictionary<string,WordData>>> wordTable = new Dictionary<char, Dictionary<int, Dictionary<string,WordData>>>();
        private static string googleOperatingWord = "A";

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
                    var baiduFrequency = 0;
                    if (tokens.Length > 2)
                        baiduFrequency = int.Parse(tokens[2]);
                    maxFrequency = Math.Max(frequency, maxFrequency);
                    if (!wordTable.ContainsKey(word[0]))
                        wordTable.Add(word[0], new Dictionary<int, Dictionary<string, WordData>>());
                    if (!wordTable[word[0]].ContainsKey(word.Length))
                        wordTable[word[0]].Add(word.Length, new Dictionary<string, WordData>());
                    wordTable[word[0]][word.Length].Add(word, new WordData() { wikiFrequency = frequency, baiduFrequency = baiduFrequency });
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
                            writer.WriteLine(stringInt.Key + "," + stringInt.Value.wikiFrequency + "," + stringInt.Value.baiduFrequency);
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
            int frequency = wordCount[word].wikiFrequency + 1;
            wordCount[word].wikiFrequency = frequency;
            if(frequency > maxFrequency)
            {
                maxFrequency = frequency;
                Console.WriteLine("Max Frequency:" + maxFrequency);
            }
            return true;
        }
        static bool SetWordFrequency(string word, int frequency)
        {
            if (!wordTable.ContainsKey(word[0]))
                return false;
            if (!wordTable[word[0]].ContainsKey(word.Length))
                return false;
            var wordCount = wordTable[word[0]][word.Length];
            if (!wordCount.ContainsKey(word))
                return false;
            wordCount[word].baiduFrequency = frequency;
            if (frequency > maxFrequency)
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
        static void LoadGooglePages(string word)
        {
            WebRequest request = WebRequest.Create("https://www.baidu.com/s?wd=" + word);
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            Console.WriteLine("Web Request: " + response.ResponseUri);
            string html = String.Empty;
            using (StreamReader sr = new StreamReader(data))
            {
                try
                {
                    html = sr.ReadToEnd();
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                    //Debugger.Break();
                    sr.Close();
                }
            }
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            //HtmlNode node = document.DocumentNode.SelectSingleNode("//*[@id=\"result-stats\"]");
            var nodes = document.DocumentNode.SelectNodes("//span[contains(@class, 'nums_text')]");
            //if (nodes.Count > 1 || nodes.Count == 0)
            //    Debugger.Break();
            //var children = nodes[0].ChildNodes;
            //string text = string.Empty;
            Console.WriteLine(nodes[0].InnerHtml);
            string numberText = string.Empty;
            foreach(char c in nodes[0].InnerHtml)
            {
                if(char.IsNumber(c))
                {
                    numberText += c;
                }
            }
            int number = int.Parse(numberText);
            SetWordFrequency(word, number);
        }
        static void LoadGooglePlace()
        {
            if (!File.Exists("save.txt")) return;
            using (StreamReader reader = new StreamReader(File.OpenRead("save.txt")))
            {
                googleOperatingWord = reader.ReadLine();
            }
        }
        static void SaveGooglePlace()
        {
            using (StreamWriter writer = new StreamWriter(File.Create("save.txt")))
            {
                writer.WriteLine(googleOperatingWord);
            }
        }
        static void Main(string[] args)
        {
            LoadWords();
            Console.WriteLine("Max Frequency: " + maxFrequency);
            Thread thread = new Thread(() =>
            {
                Stopwatch watch = new Stopwatch();
                //while (true)
                //{
                //    watch.Start();
                //    LoadRandomPage();
                //    watch.Stop();
                //    watch.Reset();
                //    elapsed += watch.Elapsed.TotalSeconds;
                //}
                if(!resetBaidu)
                    LoadGooglePlace();
                bool start = true;
                int startc = (int)(googleOperatingWord[0])-65;
                int startl = googleOperatingWord.Length;
                for(int c = start ? startc : 0; c < 26; ++c)
                {
                    char letter = (char)(c + 65);
                    if (!wordTable.ContainsKey(letter))
                        continue;
                    for(int l = start ? startl : 0; l < 20;++l)
                    {
                        if (!wordTable[letter].ContainsKey(l))
                            continue;
                        var stringInt = wordTable[letter][l];
                        int starti = -1;
                        if(start)
                            starti = Array.IndexOf(stringInt.Keys.ToArray(), googleOperatingWord);
                        for (int i = start ? starti : 0; i < stringInt.Keys.Count; ++i)
                        {
                        watch.Start();
                            googleOperatingWord = stringInt.Keys.ElementAt(i);
                            LoadGooglePages(googleOperatingWord);
                            baiduPageCount++;
                            start = false;
                            watch.Stop();
                            elapsedTimeSpan += watch.Elapsed;
                            Console.WriteLine("Total: {0}", elapsedTimeSpan);
                            Console.WriteLine("Frame: {0}", watch.Elapsed);
                            Console.WriteLine("Pages Per Minute: {0}", baiduPageCount / elapsedTimeSpan.TotalMinutes);
                            watch.Reset();
                        }
                    }
                }
            });
            thread.Start();
            var key = Console.ReadKey();
            WriteWords();
            SaveGooglePlace();
            thread.Abort();
            int n = 0;
        }
    }
}
