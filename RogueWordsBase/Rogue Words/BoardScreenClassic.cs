using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using MknGames.Split_Screen_Dungeon;
using Microsoft.Xna.Framework.Audio;

namespace MknGames.Rogue_Words
{
    public class BoardScreenClassic : RogueWordsScreen
    {
        bool drawReview = false;
        MainMenuScreenClassic mainMenu;
        public RogueWordsScreen parentScreen;
        public bool loaded = false;

        //inst player
        int playerX = 3;
        int playerY = 6;
        Point[] playerMoves = new Point[]{
            new Point(1,0),
            new Point(0,1),
            new Point(-1,0),
            new Point(0,-1)
        };
        float playerElapsed;
        public float playerDeadline = 5;

        //inst chain
        List<Tile> chainTiles = new List<Tile>();
        string chainWord;

        //inst collect
        Queue<Tile> collectionTiles = new Queue<Tile>();
        float collectionElapsed = 0;
        float collectionLength = 0.25f;

        //inst potential
        List<string> potentialWords = new List<string>();
        int lastPotentialWordCount;

        //inst scoring
        int currentCombo = 0;
        int score = 0;
        int scoreHigh = 0;

        //inst discovery
        List<string> foundWords = new List<string>();
        List<string> discoveredWords = new List<string>();
        public Dictionary<char, Dictionary<int, List<string>>> charIntStrings_discovery = new Dictionary<char, Dictionary<int, List<string>>>();

        //inst board
        int mapW = 7;
        int mapH = 12;
        public class Tile
        {
            public char letter = 'A';
            public bool consumed;
            public bool visible;
            public int chain;
            public int collectionMultiplier;
            int[] letterValueTable = new int[26]
            {
                1,//a
                3,//b
                3,//c
                2,//d
                1,//e
                4,//f
                2,//g
                4,//h
                1,//i
                8,//j
                5,//k
                1,//l
                3,//m
                1,//n
                1,//o
                3,//p
                10,//q
                1,//r
                1,//s
                1,//t
                1,//u
                4,//v
                4,//w
                8,//x
                4,//y
                10,//z
            };
            public int value
            {
                get
                {
                    return letterValueTable[letter - 65];
                }
            }
        }
        Tile[,] boardTiles;

        //inst dictionary
        public Dictionary<char, Dictionary<int, List<string>>> dictionary;

        //inst filtering
        public bool isDictionaryFiltered { get; private set; } = true;

        //inst game
        public bool requestReset = false;
        bool movesExhausted = false;
        public int assuredBranchLimit = 4;
        public float vowelChance = 50;
        char[] vowels = new char[5] { 'A', 'E', 'I', 'O', 'U' };

        //inst io
        string gameDirectory = "RogueWords/";
        string settingsFilename = "my-settings.txt";
        public static Func<string, Stream> RetrieveStream = (string path) => { return File.OpenRead(path); };

        //inst input
        Tile guiPointerReleaseTile = null;
        Point guiPointerReleaseTileCoord = new Point();
        Tile guiPointerTapTile = null;
        Point guiPointerTapTileCoord = new Point();

        //inst animation
        float noMoreWordsElapsed;
        float noMoreWordsDuration = 2;
        float totalWordDuration = 2;
        float totalWordElapsed;

        //inst sound
        public float volume = 1;
        SoundEffect placeTileSfx;
        SoundEffect bellSfx;
        SoundEffect pickupSfx;
        SoundEffect echoSfx;

        //inst test
        bool wantAutoStressTest = false;

        public BoardScreenClassic(RogueWordsGame Game, MainMenuScreenClassic main, RogueWordsScreen parent) : base(Game)
        {
            this.mainMenu = main;
            this.parentScreen = parent;

            //construct board
            ConstructBoard();
            requestReset = true;
        }

        public void SetSize(int width, int height)
        {
            mapW = width;
            mapH = height;
            ConstructBoard();
        }

        void ConstructBoard()
        {
            boardTiles = new Tile[mapW, mapH];
            for (int x = 0; x < mapW; ++x)
            {
                for (int y = 0; y < mapH; ++y)
                {
                    boardTiles[x, y] = new Tile();
                }
            }
        }

        string GetEnvironmentDirectory()
        {
#if WINDOWS
            return "";
#else
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
        }

        public string GetGameDirectory()
        {
            return Path.Combine(GetEnvironmentDirectory(), gameDirectory);
        }

        public string GetSettingsPath()
        {
            return Path.Combine(GetGameDirectory(), settingsFilename);
        }

        public void WriteSettings()
        {
            if(!Directory.Exists(GetGameDirectory()))
            {
                Directory.CreateDirectory(GetGameDirectory());
            }
            using (StreamWriter writer = new StreamWriter(File.Create(GetSettingsPath())))
            {
                Console.WriteLine("Writing to " + settingsFilename);
                writer.WriteLine("assuredBranchLimit {0}", assuredBranchLimit);
                writer.WriteLine("vowelChance {0}", vowelChance);
                writer.WriteLine("moveTimeLimit {0}", playerDeadline);
                writer.WriteLine("highScore {0}", scoreHigh);
                writer.WriteLine("isDictionaryFiltered {0}", isDictionaryFiltered);
                writer.WriteLine("volume {0}", volume);
                Console.WriteLine("Write complete");
            }
        }

        public void ReadSettings()
        {
            //load settings
            if (File.Exists(GetSettingsPath()))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(File.OpenRead(GetSettingsPath())))
                    {
                        Console.WriteLine("Reading from " + settingsFilename);
                        while (reader.EndOfStream == false)
                        {
                            string line = reader.ReadLine().Trim();
                            string[] tokens = line.Split(' ');
                            switch (tokens[0])
                            {
                                case "assuredBranchLimit":
                                    assuredBranchLimit = int.Parse(tokens[1]);
                                    break;
                                case "vowelChance":
                                    vowelChance = int.Parse(tokens[1]);
                                    break;
                                case "moveTimeLimit":
                                    playerDeadline = float.Parse(tokens[1]);
                                    break;
                                case "highScore":
                                    scoreHigh = int.Parse(tokens[1]);
                                    break;
                                case "isDictionaryFiltered":
                                    isDictionaryFiltered = bool.Parse(tokens[1]);
                                    break;
                                case "volume":
                                    volume = float.Parse(tokens[1]);
                                    break;
                            }
                        }
                        Console.WriteLine("Read complete");
                    } //end using reader
                }
                catch (Exception e)
                {
#if _DEBUG
                    throw new Exception(e.Message);
#else
                    //something went wrong? erase the file and it will be replaced with a good copy when we save.
                    Console.WriteLine("EXCEPTION: " + e.Message);
                    Console.WriteLine("Error reading settings file, deleting " + GetSettingsPath());
                    File.Delete(GetSettingsPath());
#endif
                }
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            ReadSettings();
            
            //load dictionary
            dictionary = new Dictionary<char, Dictionary<int, List<string>>>();
            bool loadFromFile = true;
            if (loadFromFile)
            {
                for (int i = 0; i < 26; ++i)
                {
                    char letter = (char)(65 + i);
                    //preproccessed file ;)
                    string filename = "Clean " + letter + " Words.txt";
                    string dir = "RogueWords/";
                    string path = dir + filename;
                    using (StreamReader reader = new StreamReader(RetrieveStream(path)))
                    {
                        Dictionary<int, List<string>> intStrings = new Dictionary<int, List<string>>();
                        dictionary.Add(letter, intStrings);
                        while (reader.EndOfStream == false)
                        {
                            string word = reader.ReadLine().Trim().ToUpper();
                            if (string.IsNullOrEmpty(word) == false)
                            {
                                char initial = word[0];
                                if (intStrings.ContainsKey(word.Length) == false)
                                    intStrings.Add(word.Length, new List<string>());
                                intStrings[word.Length].Add(word);
                            }
                        }
                    } //end using reader
                }//end for i
            } //end if loadFromFile
            else
            {
                Action<string> add = (string word) =>
                {
                    if(!dictionary.ContainsKey(word[0]))
                    {
                        dictionary.Add(char.ToUpper(word[0]), new Dictionary<int, List<string>>());
                    }
                    var charcountTable = dictionary[char.ToUpper(word[0])];
                    if (charcountTable.ContainsKey(word.Length) == false)
                        charcountTable.Add(word.Length, new List<string>());
                    charcountTable[word.Length].Add(word.ToUpper());
                };
                add("dropwisp");
                /*
                add("APPLE");
                add("BOUNCE");
                add("cat");
                add("dog");
                add("ender");
                add("fruit");
                add("goaltender");
                add("habberdashery");
                add("imbecile");
                add("jackal");
                add("karthus");
                add("lemon");
                add("monster");
                add("nectar");
                add("octopus");
                add("pencil");
                add("quiet");
                add("raster");
                add("silent");
                add("tender");
                add("underwhelm");
                add("varsity");
                add("whetstone");
                add("xylephone");
                add("zipper");*/
            }
            if (isDictionaryFiltered)
                TurnOnDictionaryFilter();
            else
                TurnOffDictionaryFilter();

            //load discovered words
            if (File.Exists(rwg.GetDiscoveryPath()))
            {
                using (StreamReader reader = new StreamReader(rwg.GetDiscoveryPath()))
                {
                    Console.WriteLine("Reading discovery from {0}", rwg.GetDiscoveryPath());
                    while(!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim().ToUpper();
                        charIntStringsAdd(charIntStrings_discovery, line);
                    }
                    Console.WriteLine("Finsh reading discovery from {0}", rwg.GetDiscoveryPath());
                }
            }

                //load sound
                placeTileSfx = game1.Content.Load<SoundEffect>("Sounds/scrabble-place-piece-0");
            bellSfx= game1.Content.Load<SoundEffect>("Sounds/bells");
            echoSfx= game1.Content.Load<SoundEffect>("Sounds/bells-echo");
            pickupSfx= game1.Content.Load<SoundEffect>("Sounds/scrabble-place-rack");

            loaded = true;
        }

        public void TurnOnDictionaryFilter()
        {
            for (int i = 0; i < rwg.curseWords.Count; ++i)
            {
                string word = rwg.curseWords[i];
                if (charIntStringsContains(dictionary, word))
                {
                    dictionary[word[0]][word.Length].Remove(word);
                }
            }
            isDictionaryFiltered = true;
        }
        public void TurnOffDictionaryFilter()
        {
            isDictionaryFiltered = false;
            for (int i = 0; i < rwg.curseWords.Count; ++i)
            {
                string word = rwg.curseWords[i];
                //do not add words that werent there before!
                if (!charIntStringsContains(dictionary, word))
                {
                    dictionary[word[0]][word.Length].Add(word);
                }
            }
        }
        public void ToggleDictionaryFilter()
        {
            if(isDictionaryFiltered)
            {
                TurnOffDictionaryFilter();
            }else
            {
                TurnOnDictionaryFilter();
            }
        }

        public override void Update(GameTime gameTime, float et)
        {
            base.Update(gameTime, et);

            //update gameplay
            if(guiPointerTapTile != guiPointerReleaseTile && 
                guiPointerTapTile != null && 
                guiPointerReleaseTile != null)
            {
                //swap tile
                Tile a = guiPointerTapTile;
                Point direction = guiPointerReleaseTileCoord - guiPointerTapTileCoord;
                Tile b = guiPointerReleaseTile;
                //Tile b = guiPointerReleaseTile;
                Tile player = boardTiles[playerX, playerY];
                int distance = Math.Abs(guiPointerReleaseTileCoord.X - guiPointerTapTileCoord.X) +
                    Math.Abs(guiPointerReleaseTileCoord.Y - guiPointerTapTileCoord.Y);
                if (distance == 1 && (a == player || b == player))
                {
                    char letterA = a.letter;
                    a.letter = b.letter;
                    b.letter = letterA;
                    //chainWord[chainWord.Length-1] = 
                    //int combo = 0;
                    Dictionary<int, List<string>> charcountTable = dictionary[chainTiles[0].letter];
                    chainWord = "";
                    int combo = 0;
                    for (int i = 0; i < chainTiles.Count; ++i)
                    {
                        chainWord += chainTiles[i].letter;
                        chainTiles[i].chain = 0;
                        if (charcountTable.ContainsKey(chainWord.Length))
                        {
                            List<string> words = charcountTable[chainWord.Length];
                            if (words.Contains(chainWord))
                            {
                                combo++;
                                for (int j = i; j >= 0; --j)
                                {
                                    chainTiles[j].chain++;
                                }
                            }
                        }
                    }
                    currentCombo = combo;
                }

            }
            //update player
            if (collectionTiles.Count == 0 && !movesExhausted)
            {
                playerElapsed += et;
            }
            bool requestMoveElimination = false;
            if (playerElapsed >= playerDeadline || wantAutoStressTest)
            {
                playerElapsed -= playerDeadline;
                requestMoveElimination = true;
            }
            Func<int, int, bool> invalidMove = (int x, int y) =>
            {
                bool result = false;
                bool inBounds = true;
                if (x < 0 || x >= mapW || y < 0 || y >= mapH)
                {
                    result = true;
                    inBounds = false;
                }
                if (inBounds && boardTiles[x, y].consumed)
                {
                    result = true;
                }
                return result;
            };
            //update moveset
            int availableMoves = 0;
            int availableMoveStartI = game1.rand.Next(playerMoves.Length);
            for (int i = 0; i < playerMoves.Length; ++i)
            {
                int movei = (availableMoveStartI + i) % playerMoves.Length;
                int x = playerX + playerMoves[movei].X;
                int y = playerY + playerMoves[movei].Y;
                if (!invalidMove(x, y))
                {
                    if (requestMoveElimination && availableMoves > 0)
                    {
                        boardTiles[x, y].consumed = true;
                        requestMoveElimination = false;
                    }
                    else
                    {
                        availableMoves++;
                    }
                }
            }
            bool requestForcedMove = false;
            if (availableMoves > 0 && requestMoveElimination)
            {
                requestForcedMove = true;
            }
            requestMoveElimination = false;
            // update moves exhausted
            if (!movesExhausted && availableMoves == 0 && chainTiles.Count > 0)
            {
                OnMovesExhausted((object)this, new EventArgs());
            }
            Point oldPosition = new Point(playerX, playerY);
            if (game1.kclick(Keys.Right))
            {
                playerX++;
            }
            if (game1.kclick(Keys.Left))
            {
                playerX--;
            }
            if (game1.kclick(Keys.Up))
            {
                playerY--;
            }
            if (game1.kclick(Keys.Down))
            {
                playerY++;
            }
            // move player
            if(guiPointerReleaseTile!=null&&guiPointerReleaseTile ==guiPointerTapTile)
            {
                playerX = guiPointerReleaseTileCoord.X;
                playerY = guiPointerReleaseTileCoord.Y;
            }
            if (game1.kclick(Keys.R))
            {
                requestReset = true;
            }
            bool requestConsumeCurrentTile = false;
            // on reset
            if (requestReset) // reset
            {
                requestReset = false;

                //reset game
                movesExhausted = false;

                //reset score
                scoreHigh = Math.Max(score, scoreHigh);
                score = 0;

                //reset potential
                potentialWords.Clear();
                lastPotentialWordCount = 0;

                //reset chain (this must be reset before PullLetter is called again)
                chainTiles.Clear();
                chainWord = "";

                //reset collection
                collectionTiles.Clear();
                collectionElapsed = 0;

                //reset board
                foreach (Tile t in boardTiles)
                {
                    t.consumed = false;
                    t.visible = false;
                    t.chain = -1;
                    t.letter = '%';// (char)((int)'A' + game1.rand.Next(26));
                    t.collectionMultiplier = 0;
                }

                //reset draw
                drawReview = false;

                //reset player
                playerX = mapW / 2;
                playerY = mapH / 2;
                oldPosition.X = playerX;
                oldPosition.Y = playerY;
                requestConsumeCurrentTile = true;

                var randCharTable = dictionary[dictionary.Keys.ElementAt(game1.rand.Next(dictionary.Keys.Count))];
                var randCharCountTable = randCharTable[randCharTable.Keys.ElementAt(game1.rand.Next(randCharTable.Keys.Count))];
                boardTiles[playerX, playerY].letter = randCharCountTable[game1.rand.Next(randCharCountTable.Count)][0];

                playerElapsed = 0;

                //reset score
                currentCombo = 0;

                //reset discovered words
                foundWords.Clear();
                discoveredWords.Clear();
            }
            bool playerMoved = oldPosition.X != playerX || oldPosition.Y != playerY;
            if (!playerMoved && requestForcedMove)
            {
                int start = game1.rand.Next(playerMoves.Length);
                for (int i = 0; i < 4; ++i)
                {
                    int movei = (start + i) % playerMoves.Length;
                    Point move = playerMoves[movei];
                    if (invalidMove(playerX + move.X, playerY + move.Y) == false)
                    {
                        playerX += move.X;
                        playerY += move.Y;
                        playerMoved = true;
                        break;
                    }
                }
                if (!playerMoved)
                {
                    Console.WriteLine("Force move requested but no moves found!");
                }
            }
            if (playerMoved)
            {
                if (invalidMove(playerX, playerY))
                {

                    playerX = oldPosition.X;
                    playerY = oldPosition.Y;
                }
                else
                {
                    requestConsumeCurrentTile = true;
                    playerElapsed = 0;
                }
            }

            //update chain
            if (requestConsumeCurrentTile)
            {
                placeTileSfx.Play(volume, 0, 0);
                Tile T = boardTiles[playerX, playerY];
                T.consumed = true;
                T.chain = 0;
                chainTiles.Add(T);
                chainWord += T.letter;
                Dictionary<int, List<String>> charcountTable = dictionary[chainWord[0]]; //table with an entire list of words associated with an integer referring to a word's character length
                if (charcountTable.ContainsKey(chainWord.Length))
                {
                    List<string> matchingWords = charcountTable[chainWord.Length];
                    // update discovered words
                    if (matchingWords.Contains(chainWord))
                    {
                        // found word
                        foundWords.Add(chainWord);
                        // discovered word
                        if (!charIntStringsContains(charIntStrings_discovery, chainWord))
                        {
                            charIntStringsAdd(charIntStrings_discovery, chainWord);
                            discoveredWords.Add(chainWord);
                        }
                        foreach (Tile t in chainTiles)
                        {
                            t.chain++;
                        }
                        currentCombo++;
                        //post word found
                        //float pitch = ComboToPitch(currentCombo);
                        //bellSfx.Play(1, pitch, 0);
                    }
                }
                
                    UpdateWordPotential(charcountTable);
                if(potentialWords.Count == 1 && potentialWords[0].Length == chainWord.Length)
                {
                    noMoreWordsElapsed = 0;
                }
                if (potentialWords.Count == 0)
                {
                    //if(chainTiles[chainTiles.Count-1].chain > 0)
                    //{
                    //    totalWordElapsed = 0;
                    //    score += 25;
                    //}
                    //pre word finish
                    Collect(1);
                    UpdateWordPotential(dictionary[chainWord[0]]);
                    noMoreWordsElapsed = noMoreWordsDuration;
                }
                
            }

            //update collection
            if(collectionTiles.Count > 0)
            {
                if(collectionElapsed > collectionLength)
                {
                    collectionElapsed -= collectionLength;
                    Tile t = collectionTiles.Dequeue();
                    t.chain = -1;
                    score += t.value * t.collectionMultiplier;
                    OnPostTileCollected((object)this, new EventArgs());
                }
                collectionElapsed += et;
            }

            //update board
            Tile[] oldVisible = new Tile[playerMoves.Length];
            Tile[] newVisible = new Tile[playerMoves.Length];
            for (int i = 0; i < playerMoves.Length; ++i)
            {
                Func<int, int, Tile> getVisible = (int x, int y) =>
                {
                    if (invalidMove(x, y))
                        return null;
                    return boardTiles[x, y];
                };
                oldVisible[i] = getVisible(oldPosition.X + playerMoves[i].X, oldPosition.Y + playerMoves[i].Y);
                newVisible[i] = getVisible(playerX + playerMoves[i].X, playerY + playerMoves[i].Y);
            }
            //int nextPossiblei = 0;
            int assuredBrachCount = 0;
            int offsetVis = game1.rand.Next();
            for (int i = 0; i < oldVisible.Length; ++i)
            {
                int v = (offsetVis + i) % oldVisible.Length; //visible tile index
                //if the old does not exist in new, consume it
                Tile oldvis = oldVisible[v];
                if (oldvis != null && !newVisible.Contains(oldvis))
                {
                    oldvis.consumed = true;
                    oldvis.visible = false;
                }
                Tile newvis = newVisible[v];
                if (newvis != null)
                {
                    string nextPossible = null;
                    if (assuredBrachCount < assuredBranchLimit &&
                        (potentialWords.Count > 1 ||
                        (potentialWords.Count == 1 && potentialWords[0].Length > chainWord.Length)))
                    {
                        //nextPossible = potentialWords[nextPossiblei++];
                        //if (nextPossiblei >= potentialWords.Count)
                        //    nextPossiblei = 0;
                        //if (nextPossible.Length == chainWord.Length)
                        //{
                        //    nextPossible = potentialWords[nextPossiblei++];
                        //    if (nextPossiblei >= potentialWords.Count)
                        //        nextPossiblei = 0;
                        //}
                        int attempts = 0;
                        string randomNab = null;
                        do
                        {
                            randomNab = potentialWords[game1.rand.Next(potentialWords.Count)];
                            if (randomNab.Length != chainWord.Length)
                            {
                                nextPossible = randomNab;
                                break;
                            }
                            attempts++;
                        } while (attempts < potentialWords.Count);
                    }
                    if (!newvis.visible)
                    {
                        newvis.letter = PullLetter(nextPossible);
                        if(nextPossible != null)
                        {
                            assuredBrachCount++;
                        }
                    }
                    newvis.visible = true;
                }
            }
        }

        private void OnPostTileCollected(object v, EventArgs eventArgs)
        {
            pickupSfx.Play(volume, 0, 0);
        }

        public float ComboToPitch(int combo)
        {
            float v = (float)combo / 6.0f;
            if(v > 5.0f/6f)
            {
                v = 5.0f / 6f + v / 10f;
            }
            return MathHelper.Lerp(-0.5f, 1.0f, v);
        }

        private void OnMovesExhausted(object v, EventArgs eventArgs)
        {
            Collect(0);
            movesExhausted = true;
            //requestReset = true;
            drawReview = true;
            WriteSettings();
            if (wantAutoStressTest)
            {
                requestReset = true;
                collectionLength = 0;// game1.randf(0.25f);
                vowelChance = game1.rand.Next(100);
                assuredBranchLimit = game1.rand.Next(5);
                SetSize(game1.rand.Next(1, 30), game1.rand.Next(1, 30));
            }
        }

        //pull letter
        public char PullLetter(string potentialWord = null)
        {
            if (!string.IsNullOrEmpty(potentialWord))
            {
                return potentialWord[chainWord.Length];
            }
            if (game1.rand.Next(100) < vowelChance)
            {
                return vowels[game1.rand.Next(vowels.Length)];
            }
            return dictionary.Keys.ElementAt(game1.rand.Next(dictionary.Keys.Count));
        }
        public void UpdateWordPotential(Dictionary<int, List<string>> charcountTable)
        {
            lastPotentialWordCount = potentialWords.Count;
            potentialWords.Clear();
            int s = game1.rand.Next(charcountTable.Keys.Count); //starting index
            for (int i = 0; i < charcountTable.Keys.Count; ++i) //use for loop to iterate
            {
                int k = (s + i) % charcountTable.Keys.Count; //index of length key
                int length = charcountTable.Keys.ElementAt(k); //integer key referring to a word's length
                if (length < chainWord.Length) //discard words shorter than the current
                    continue;
                List<string> words = charcountTable[length]; //potential words
                foreach (string word in words)
                {
                    bool match = true; //roots match
                    for (int j = 0; j < chainWord.Length; ++j)
                    {
                        if (word[j] != chainWord[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        potentialWords.Add(word); //a 100% match is still a potential word
                    }
                }
                //if (potentialWords.Count >= 4) //small optimization we only need enough for each of the four directions
                //{
                //    break;
                //}
            }//end foreach word
        }
        /// <summary>
        /// Removes tiles from the current chain and places them into a queue for collection.
        /// </summary>
        /// <param name="leftover">The amount of tiles to keep in the current chain and not queoe for collection</param>
        /// <returns></returns>
        public bool Collect(int leftover)
        {
            //tally up and remove each tile in the chain except the current one (the last one)
            int removeCount = chainTiles.Count - leftover;
            bool collectionOccurred = false;
            for (int i = 0; i < removeCount; ++i)
            {
                int c = chainTiles.Count - 1 - leftover;
                Tile t = chainTiles[c];
                //score += t.value * scoreMultiplier;
                if (t.chain > 0)
                {
                    t.collectionMultiplier = currentCombo;
                    t.chain = currentCombo;
                }
                chainTiles.RemoveAt(c);
                collectionTiles.Enqueue(t);
                collectionOccurred = true;
            }
            chainWord = chainWord.Substring(chainWord.Length - leftover, leftover);
            currentCombo = 0;
            return collectionOccurred;
        }
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            rwg.activeScreen = parentScreen;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (requestReset)
                return;

            base.Draw(gameTime, spriteBatch);


            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;

            game1.GraphicsDevice.Clear(Color.Magenta);

            game1.drawSquare(ViewportRect, Color.LimeGreen, 0);

            Rectangle playRect = Split_Screen_Dungeon.Backpack.percentage(ViewportRect, 0, 0.15f, 1, 0.85f);
            if (playRect.Bottom != ViewportRect.Bottom)
                playRect.Height++;
            game1.drawSquare(playRect, monochrome(0.2f), 0);

            // draw board
            Vector2 prcenter = playRect.Center.ToVector2();
            float tileH = Split_Screen_Dungeon.Backpack.percentageH(playRect, 1f / 13f);
            float tileW = tileH;
            Vector2 tileSize = new Vector2(tileW, tileH);
            Vector2 mapSize = new Vector2(mapW, mapH) * tileSize;
            Vector2 mapCenter = mapSize / 2;
            Vector2 playerCenter = new Vector2(playerX, playerY) * tileSize + tileSize / 2;
            Vector2 playerOffset = playerCenter - mapCenter;
            if (mapSize.X <= playRect.Width)
                playerOffset.X = 0;
            if (mapSize.Y <= playRect.Height)
                playerOffset.Y = 0;
            Rectangle container = game1.centeredRect(prcenter - playerOffset, mapSize.X, mapSize.Y);
            if (mapSize.X > playRect.Width)
            {
                if (container.Left > playRect.Left) container.X -= container.Left - playRect.Left;
                if (container.Right < playRect.Right) container.X -= container.Right - playRect.Right;
            }
            if (mapSize.Y > playRect.Height)
            {
                if (container.Top > playRect.Top) container.Y -= container.Top - playRect.Top;
                if (container.Bottom < playRect.Bottom) container.Y -= container.Bottom - playRect.Bottom;
            }

            Vector2 tileOffset = container.Location.ToVector2() + tileSize/2;
            // draw tiles
            Vector2[] tileCorners = new Vector2[4] {
                Vector2.Normalize(new Vector2(-1,-1)),
                Vector2.Normalize(new Vector2(1,-1)),
                Vector2.Normalize(new Vector2(1,1)),
                Vector2.Normalize(new Vector2(-1,1))
            };
            Vector2[] tileSides = new Vector2[4] {
                Vector2.Normalize(new Vector2(-1,0)),
                Vector2.Normalize(new Vector2(1,0)),
                Vector2.Normalize(new Vector2(0,1)),
                Vector2.Normalize(new Vector2(0,-1))
            };
            guiPointerReleaseTile = null; // reset gui pointer release tile
            if(pointerUp() && pointerUpOld())
                guiPointerTapTile = null;
            for (int x = 0; x < mapW; ++x)
            {
                for (int y = 0; y < mapH; ++y)
                {
                    Tile T = boardTiles[x, y];
                    Color bg = monochrome(0);
                    Color fg = monochrome(0.5f);
                    bool drawInfo = false;
                    bool drawRoundedSquare = false;
                    bool drawFrame = false;
                    if (T.chain > -1)
                    {
                        bg =
                            T.chain == 0 ? Color.SaddleBrown :
                            T.chain == 1 ? Color.LimeGreen :
                            T.chain == 2 ? Color.Blue :
                            T.chain == 3 ? Color.Yellow :
                            T.chain == 4 ? Color.Orange :
                            T.chain == 5 ? Color.Red :
                            Color.Magenta;
                        fg = monochrome(1);
                        drawInfo = true;
                        drawFrame = true;
                    }
                    else
                    {
                        if (T.consumed || T.visible)
                        {
                            drawInfo = true;
                            if (T.consumed)
                                drawFrame = true;
                            else
                                drawRoundedSquare = true;
                        }
                        if (T.visible && !T.consumed)
                        {
                            bg = monochrome(0.5f);
                            fg = monochrome(1.0f);
                        }
                    }
                    // draw tile
                    Vector2 pos = tileOffset + new Vector2(x * tileW, y * tileH);
                    if (drawRoundedSquare)
                    {
                        game1.drawSquare(pos, monochrome(0), 0, tileW, tileH);
                        float radius = tileW / 8;
                        float diagonal = ((tileSize / 2) - new Vector2(radius)).Length();
                        for (int i = 0; i < tileCorners.Length; ++i)
                        {
                            game1.drawCircleCentered(pos + tileCorners[i] * diagonal, new Vector2(radius, radius) * 2, bg);
                        }
                        game1.drawSquare(pos, bg, 0, tileW - radius * 2, tileH);
                        game1.drawSquare(pos, bg, 0, tileW, tileH - radius * 2);
                    }
                    else
                    {
                        game1.drawSquare(pos, bg, 0, tileW, tileH);
                    }
                    if (drawInfo)
                    {
                        if (drawFrame)
                        {
                            game1.drawFrame(pos, fg, tileW, tileH, 1);
                        }

                        //draw value
                        Rectangle r = game1.centeredRect(pos, tileW, tileH);
                        if(pointerRelease() && r.Contains(pointer()))
                        {
                            guiPointerReleaseTile = T;
                            guiPointerReleaseTileCoord.X = x;
                            guiPointerReleaseTileCoord.Y = y;
                        }
                        if (pointerTap() && r.Contains(pointer()))
                        {
                            guiPointerTapTile = T;
                            guiPointerTapTileCoord.X = x;
                            guiPointerTapTileCoord.Y = y;
                        }
                        Rectangle ra = Split_Screen_Dungeon.Backpack.percentage(r, 0, 3f / 4f, 1, 1f / 4f);
                        game1.drawString(game1.defaultLargerFont, "" + T.value, ra, fg, new Vector2(0, 1), true);

                        //draw letter
                        Rectangle rb = Split_Screen_Dungeon.Backpack.percentage(r, 0, 0, 1, 3f / 4f);
                        char letter = T.letter;
                        //if (T != chainTiles[0])
                        //    letter = char.ToLower(letter);
                        game1.drawString(game1.defaultLargerFont, "" + letter, rb.Size.ToVector2(), rb.Location.ToVector2(), fg, new Vector2(0.5f), true);
                    }
                }
            }

            //draw player
            if (!movesExhausted)
            {
                float progress = playerElapsed / playerDeadline;
                float radius = (tileW / 2) * (1 - progress);
                game1.drawNgon(tileOffset + new Vector2(playerX * tileW, playerY * tileH), monochrome(1), 0,
                    6, radius, 1);
            }

            Rectangle overheadRect = Split_Screen_Dungeon.Backpack.percentage(ViewportRect, 0, 0, 1, 0.1f);
            game1.drawSquare(overheadRect, monochrome(0.5f), 0);

            //draw multiplier
            Rectf multiplierRect = Split_Screen_Dungeon.Backpack.percentagef(overheadRect, 0, 0, 1f / 6f, 1);
            {
                Rectf r = multiplierRect;
                r.X -= r.Width;
                r.Y -= r.Height;
                r.Width *= 2;
                r.Height *= 2;
                game1.drawCircle(r, monochrome(0.8f));
            }
            Rectf multirtxt = Split_Screen_Dungeon.Backpack.percentagef(multiplierRect, 0, 0.3f, 1, 0.4f);
            Color multic =
                            currentCombo == 0 ? Color.Black :
                            currentCombo == 1 ? Color.LimeGreen :
                            currentCombo == 2 ? Color.Blue :
                            currentCombo == 3 ? Color.Yellow :
                            currentCombo == 4 ? Color.Orange :
                            currentCombo == 5 ? Color.Red :
                            Color.Magenta;
            game1.drawStringf(game1.defaultLargerFont, currentCombo + "x", multirtxt, multic, new Vector2(0.25f), true);
            if(pointerTap() && multiplierRect.ContainsPoint( pointer()))
            {
                requestReset = true;
            }


            //draw score
            Rectangle scoreRect = Split_Screen_Dungeon.Backpack.percentage(overheadRect, 5f / 6f, 0, 1f / 6f, 1);
            {
                Rectf r = scoreRect;
                //r.X += r.Width;
                r.Y -= r.Height;
                r.Width *= 2;
                r.Height *= 2;
                game1.drawCircle(r, monochrome(0.8f));
            }
            Rectangle scoreRectTxt = Split_Screen_Dungeon.Backpack.percentage(scoreRect, 0, 0.3f, 1, 0.4f);
            game1.drawString(game1.defaultLargerFont, "" + score, scoreRectTxt, monochrome(0), new Vector2(0.75f,0), true);
            if (scoreRect.Contains(pointer()) && pointerTap())
            {
                drawReview = !drawReview;
            }

            Rectf middleRect = overheadRect;
            middleRect.Width -= scoreRect.Width;
            middleRect.Width -= multiplierRect.Width;
            middleRect.X += scoreRect.Width;
            Rectf returnBtn = Backpack.percentagef(middleRect, 0f, 0.25f, 0.2f, .5f);
            game1.drawSquare(returnBtn, monochrome(0.2f), 0);
            game1.drawStringf(game1.defaultLargerFont, "back", returnBtn, monochrome(1.0f), new Vector2(0.5f), true, 1);
            if (returnBtn.ContainsPoint(pointer()) && pointerRelease())
            {
                rwg.activeScreen = parentScreen;
            }

            // draw chain word
            Rectf mra = Split_Screen_Dungeon.Backpack.percentagef(middleRect, 0, 0, 1, 2f / 3f);
            //Rectangle innermra = Split_Screen_Dungeon.Backpack.percentage(mra, 0.05f,0.05f,0.9f,0.9f);
            {
                Rectf r = game1.CalculateTextContainer(game1.defaultLargerFont, chainWord, mra, monochrome(1), new Vector2(0.5f), true);
                game1.drawSquare(r, monochrome(0), 0);
                game1.drawStringf(game1.defaultLargerFont, chainWord, r, monochrome(1), new Vector2(0.5f), true);
            }
            Rectf mrb = middleRect;
            mrb.Height -= mra.Height;
            mrb.Y += mra.Height;
            
            for (float i = 0; i < currentCombo; ++i)
            {
                float f = (float)currentCombo;
                Rectf r = Split_Screen_Dungeon.Backpack.percentagef(mrb, i / f, 0, 1 / f, 1);
                //string text = i == 0 ? "DE" : i == 1 ? "DEV" : i == 2 ? "DEVISE" : "DEVISES";
                string text = foundWords[foundWords.Count-1-(int)i];
                Rectf textbox = game1.CalculateTextContainer(game1.defaultLargerFont, text, r, monochrome(1), new Vector2(0.5f), true);
                game1.drawSquare(textbox, monochrome(0), 0);
                game1.drawStringf(game1.defaultLargerFont, text, textbox, monochrome(1), new Vector2(0.5f), true);
            }

            // draw alert
                Rectangle alertr = Split_Screen_Dungeon.Backpack.percentage(ViewportRect, 0, 0.1f, 1, 0.05f);
            string alertText = "";
            if (noMoreWordsElapsed <= noMoreWordsDuration)
            {
                alertText += "NO MORE WORDS";
                //alertr.Width = (int)mapSize.X;
                //alertr.X += (playRect.Width - alertr.Width) / 2;
                //game1.drawString(game1.defaultLargerFont, "NO MORE WORDS", alertr, monochrome(1), new Vector2(0.5f, 0), true);
                noMoreWordsElapsed += et;
            }
            if (totalWordElapsed <= totalWordDuration)
            {
                alertText += "TOTAL WORD: +25";
                //Rectangle alertr2 = alertr;
                //alertr2.Y += alertr2.Height;
                //game1.drawString(game1.defaultLargerFont, "TOTAL WORD: +25", alertr2, Color.Green, new Vector2(0.5f, 0), true);
                totalWordElapsed += et;
            }
            game1.drawSquare(alertr, monochrome(0.0f), 0);
            game1.drawFrame(alertr, monochrome(0.5f), 1);
            game1.drawString(game1.defaultLargerFont, alertText, alertr, monochrome(1.0f), new Vector2(0.5f), true);

            if (drawReview)
            {
                Rectangle reviewRect = playRect;
                float border = Split_Screen_Dungeon.Backpack.percentageH(playRect, 1f / 32f) / 2;
                reviewRect.Inflate(-border, -border);
                float lightv = 0.8f;
                game1.drawSquare(reviewRect, monochrome(lightv, 0.8f), 0);
                // draw discovered words
                for (float i = 0; i < foundWords.Count + 2; ++i)
                {
                    Rectangle r = Split_Screen_Dungeon.Backpack.percentage(reviewRect, 0, i / 25f, 1, 1f / 25f);
                    Rectangle drawr = r;
                    string text = "Tap the top left area to restart";
                    string highscoreText = "NEW HIGH SCORE";
                    bool doHighscoreText = false;
                    if (i == 1)
                    {
                        text = "Score: " + score;
                        doHighscoreText = score > scoreHigh;
                        if(!doHighscoreText)
                        {
                            text += ", High Score: " + scoreHigh;
                        }
                    }
                    if (i > 1)
                    {
                        int d = (int)i - 2; //discovered word index
                        text = d + ". " + foundWords[d];
                        if(discoveredWords.Contains(foundWords[d]))
                        {
                            doHighscoreText = true;
                            highscoreText = "new";
                        }
                        //int a = (int)i % 17;
                        //int b = a - 2;
                        //if (i % 18 == 17)
                        //{
                        //    text = "";
                        //}
                        //else
                        //{
                        //    drawr = Split_Screen_Dungeon.Backpack.percentage(r, 1f / 8f, 0, 7f / 8f, 1f);
                        //    if (i >= 17)
                        //    {
                        //        b += 1;
                        //    }
                        //    else
                        //    {
                        //        doHighscoreText = true;
                        //        highscoreText = "_NEW_";
                        //    }
                        //    text = b + ". lomein";
                        //}
                    }
                    if (doHighscoreText)
                    {
                        Rectangle newr = Split_Screen_Dungeon.Backpack.percentage(drawr, 0.5f, 0, 0.5f, 1);
                        game1.drawString(game1.defaultLargerFont, highscoreText, newr, Color.Yellow, new Vector2(0.5f, 0), true);
                    }
                    game1.drawString(game1.defaultLargerFont, text, drawr, monochrome(0), Vector2.Zero, true);
                }
            }
        }
    }
}