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
using RogueWordsBase;
using RogueWordsBase.RogueWords.GameModes;
using RogueWordsBase.RogueWords;

namespace MknGames.Rogue_Words
{
    public class WordData
    {
        public int commonLevel;
    }
    public class BoardScreenClassic : RogueWordsScreen
    {
        bool drawReview = false;
        MainMenuScreenClassic mainMenu;
        public RogueWordsScreen parentScreen;
        public bool loaded = false;

        //inst player
        public bool playerMoved;
        public int playerX = 3;
        public int playerY = 6;
        public Point oldPlayerPosition;
        Point[] playerMoves = new Point[]{
            new Point(1,0),
            new Point(0,1),
            new Point(-1,0),
            new Point(0,-1)
        };
        float playerElapsed;
        public float playerDeadline = 5;

        //inst chain
        public List<Tile> chainTiles = new List<Tile>();
        public string chainWord;

        //inst collect
        public Queue<Tile> collectionTiles = new Queue<Tile>();
        public float collectionElapsed = 0;
        public float collectionDuration = 0.25f;

        //inst potential
        List<string> potentialWords = new List<string>();
        Dictionary<char, List<string>> potentialWordTable = new Dictionary<char, List<string>>();
        int lastPotentialWordCount;

        //inst scoring
        int currentCombo = 0;
        public int score = 0;
        int scoreHigh = 0;

        //inst discovery
        List<string> discoveredWords = new List<string>();

        //inst board
        public int mapW = 7;
        public int mapH = 12;
        public Tile[,] boardTiles;

        //inst dictionary
        public Dictionary<char, Dictionary<int, Dictionary<string, WordData>>> dictionary;
        Dictionary<int, List<string>> commonWords;
        public int[] commonSizes = { 1, 2, 5, 10, 50 };
        public int targetCommonLevel = 0;

        //inst game
        public bool requestReset = false;
        public bool consumeOldVisibleFlag = true;
        public bool revealNewVisibleFlag = true;
        public bool consumeCurrentTileFlag = true;
        public bool wordBuildFlag = true;
        public bool applyMultiplierFlag = true;
        public bool allowChainingFlag = true;
        public bool collectOnWordExhaustionFlag = true;
        public bool drawDiscoveredWordsFlag = true;
        bool movesExhausted = false;
        public int assuredBranchLimit = 4;
        public float vowelChance = 50;
        char[] vowels = new char[5] { 'A', 'E', 'I', 'O', 'U' };
        public Color[] chainColors = new Color[] {
            Color.SaddleBrown ,
            Color.LimeGreen ,
            Color.Blue ,
            Color.Yellow ,
            Color.Orange ,
            Color.Red
        };
        RogueWordsGameMode gameMode;

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
        SoundEffect placeTileSfx;
        SoundEffect bellSfx;
        SoundEffect pickupSfx;
        SoundEffect echoSfx;

        //inst gui
        public float playRectY = 0.15f;
        public float playRectHeight = 0.85f;
        private Rectangle playRect;
        private float tileH;
        private float tileW;
        private Vector2 tileOffset;
        private Rectangle overheadRect;
        private Rectf multiplierRect;
        private Rectangle scoreRect;
        private Rectf returnBtn;
        private Rectf middleRect;
        public bool drawMultiplierFlag=true;

        public BoardScreenClassic(RogueWordsGame Game, MainMenuScreenClassic main, RogueWordsScreen parent) : base(Game)
        {
            this.mainMenu = main;
            this.parentScreen = parent;
            //gameMode = new RogueGameMode(this);
            gameMode = new QuestGameMode(this);

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

        public void ConstructBoard()
        {
            boardTiles = new Tile[mapW, mapH];
            for (int x = 0; x < mapW; ++x)
            {
                for (int y = 0; y < mapH; ++y)
                {
                    boardTiles[x, y] = new Tile(x,y);
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

        string GetGameDirectory()
        {
            return Path.Combine(GetEnvironmentDirectory(), gameDirectory);
        }

        string GetSettingsPath()
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

        void AddCommonWord(string word, int frequency)
        {
            for (int i = 0; i < commonSizes.Length; ++i)
            {
                int level = commonSizes.Length - i - 1;
                if (!commonWords.ContainsKey(level))
                    commonWords.Add(level, new List<string>());
                if (frequency < commonSizes[i])
                {
                    commonWords[level].Add(word);
                    dictionary[word[0]][word.Length][word].commonLevel = level;
                    break;
                }
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            ReadSettings();

            //load dictionary
            dictionary = new Dictionary<char, Dictionary<int, Dictionary<string, WordData>>>();
            commonWords = new Dictionary<int, List<string>>();
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
                        while (reader.EndOfStream == false)
                        {
                            string line = reader.ReadLine();
                            string[] tokens = line.Split(',');
                            string word = tokens[0].ToUpper();
                            if (string.IsNullOrEmpty(word) == false)
                            {
                                char initial = word[0];
                                if (initial != letter)
                                    continue;
                                if (dictionary.ContainsKey(initial) == false)
                                    dictionary.Add(initial, new Dictionary<int, Dictionary<string, WordData>>());
                                Dictionary<int, Dictionary<string, WordData>> pages = dictionary[initial];
                                if (pages.ContainsKey(word.Length) == false)
                                    pages.Add(word.Length, new Dictionary<string, WordData>());
                                Dictionary<string,WordData> words = pages[word.Length];
                                words.Add(word, new WordData());
                            }
                            int frequency = 0;
                            if (tokens.Length > 1)
                                frequency = int.Parse(tokens[1]);
                            AddCommonWord(word, frequency);
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
                        dictionary.Add(char.ToUpper(word[0]), new Dictionary<int, Dictionary<string, WordData>>());
                    }
                    var charcountTable = dictionary[char.ToUpper(word[0])];
                    if (charcountTable.ContainsKey(word.Length) == false)
                        charcountTable.Add(word.Length, new Dictionary<string, WordData>());
                    charcountTable[word.Length].Add(word.ToUpper(), new WordData());
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

            //load sound
            try
            {
                placeTileSfx = game1.Content.Load<SoundEffect>("Sounds/scrabble-place-piece-0");
                bellSfx = game1.Content.Load<SoundEffect>("Sounds/bells");
                echoSfx = game1.Content.Load<SoundEffect>("Sounds/bells-echo");
                pickupSfx = game1.Content.Load<SoundEffect>("Sounds/scrabble-place-rack");
            }catch(Exception e)
            {
                System.Diagnostics.Debugger.Break();
            }

            gameMode.OnLoadContent();

            loaded = true;
        }

        public override void Update(GameTime gameTime, float et)
        {
            base.Update(gameTime, et);

            gameMode.OnUpdate();

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
                    if (chainTiles.Count > 0)
                    {
                    Dictionary<int, Dictionary<string, WordData>> charcountTable = dictionary[chainTiles[0].letter];
                        chainWord = "";
                        int combo = 0;
                        for (int i = 0; i < chainTiles.Count; ++i)
                        {
                            chainWord += chainTiles[i].letter;
                            chainTiles[i].chain = 0;
                            if (charcountTable.ContainsKey(chainWord.Length))
                            {
                                Dictionary<string, WordData> words = charcountTable[chainWord.Length];
                                if (words.ContainsKey(chainWord))
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

            }
            //update player
            if (collectionTiles.Count == 0 && !movesExhausted)
            {
                playerElapsed += et;
            }
            bool requestMoveElimination = false;
            if (playerElapsed >= playerDeadline)
            {
                playerElapsed -= playerDeadline;
                requestMoveElimination = true;
            }
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
            oldPlayerPosition = new Point(playerX, playerY);
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
                int deltaX = playerX - guiPointerReleaseTileCoord.X;
                int deltaY = playerY - guiPointerReleaseTileCoord.Y;
                if (Math.Abs(deltaX) + Math.Abs(deltaY) == 1)
                {
                    playerX = guiPointerReleaseTileCoord.X;
                    playerY = guiPointerReleaseTileCoord.Y;
                }
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
                potentialWordTable.Clear();
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
                oldPlayerPosition.X = playerX;
                oldPlayerPosition.Y = playerY;
                requestConsumeCurrentTile = true;

                var randCharTable = dictionary[dictionary.Keys.ElementAt(game1.rand.Next(dictionary.Keys.Count))];
                var randCharCountTable = randCharTable[randCharTable.Keys.ElementAt(game1.rand.Next(randCharTable.Keys.Count))];
                boardTiles[playerX, playerY].letter = randCharCountTable.Keys.ElementAt(game1.rand.Next(randCharCountTable.Keys.Count))[0];

                playerElapsed = 0;

                //reset score
                currentCombo = 0;

                //reset discovered words
                discoveredWords.Clear();

                gameMode.OnReset();
            }
            playerMoved = oldPlayerPosition.X != playerX || oldPlayerPosition.Y != playerY;
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

                    playerX = oldPlayerPosition.X;
                    playerY = oldPlayerPosition.Y;
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
                placeTileSfx.Play();
                Tile T = boardTiles[playerX, playerY];
                if(consumeCurrentTileFlag)
                    T.consumed = true;
                if (wordBuildFlag)
                {
                    T.chain = 0;
                    chainTiles.Add(T);
                    chainWord += T.letter;

                    Dictionary<int, Dictionary<string, WordData>> charcountTable = dictionary[chainWord[0]]; //table with an entire list of words associated with an integer referring to a word's character length
                    if (charcountTable.ContainsKey(chainWord.Length))
                    {
                        Dictionary<string, WordData> matchingWords = charcountTable[chainWord.Length];
                        // update discovered words
                        if (matchingWords.ContainsKey(chainWord))
                        {
                            discoveredWords.Add(chainWord);
                                foreach (Tile t in chainTiles)
                            {
                                    t.chain++;
                                if (!allowChainingFlag)
                                {
                                    t.chain = Math.Min(1, t.chain);
                                }
                            }
                                currentCombo++;
                            //post word found
                            //float pitch = ComboToPitch(currentCombo);
                            //bellSfx.Play(1, pitch, 0);
                        }
                    }

                    UpdateWordPotential(charcountTable);
                    if (potentialWords.Count == 1 && potentialWords[0].Length == chainWord.Length)
                    {
                        noMoreWordsElapsed = 0;
                    }
                    if (potentialWords.Count == 0 && collectOnWordExhaustionFlag)
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
            }

            //update collection
            if(collectionTiles.Count > 0)
            {
                if(collectionElapsed > collectionDuration)
                {
                    collectionElapsed -= collectionDuration;
                    Tile t = collectionTiles.Dequeue();
                    int value = t.value;
                    if (applyMultiplierFlag)
                        value *= t.chain;
                    if (t.chain == 0)
                        value = -value;
                    t.chain = -1;
                    score += value;
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
                oldVisible[i] = getVisible(oldPlayerPosition.X + playerMoves[i].X, oldPlayerPosition.Y + playerMoves[i].Y);
                newVisible[i] = getVisible(playerX + playerMoves[i].X, playerY + playerMoves[i].Y);
            }
            //int nextPossiblei = 0;
            int assuredBrachCount = 0;
            int offsetVis = game1.rand.Next();
            List<char> existingLetters = new List<char>();
            for (int i = 0; i < oldVisible.Length; ++i)
            {
                int v = (offsetVis + i) % oldVisible.Length; //visible tile index
                //if the old does not exist in new, consume it
                Tile oldvis = oldVisible[v];
                if (oldvis != null && !newVisible.Contains(oldvis) && consumeOldVisibleFlag)
                {
                    oldvis.consumed = true;
                    oldvis.visible = false;
                }
                Tile newvis = newVisible[v];
                if (newvis != null && !newvis.visible)
                {
                    string nextPossible = null;
                    if (assuredBrachCount < assuredBranchLimit &&
                        (potentialWords.Count > 1 ||
                        (potentialWords.Count == 1 && potentialWords[0].Length > chainWord.Length)))
                    {
                        nextPossible = GetNextPossibleWord(existingLetters);
                    }
                    if (!newvis.visible)
                    {
                        newvis.letter = PullLetter(nextPossible);
                        if(nextPossible != null)
                        {
                            existingLetters.Add(newvis.letter);
                            assuredBrachCount++;
                        }
                    }
                    if (revealNewVisibleFlag)
                    {
                        newvis.visible = true;
                    }
                }
            }
            updateGUI();
            gameMode.OnPostUpdate();
        }
        public string GetNextPossibleWord(List<char> existingLetters)
        {
            string nextPossible = null;
            //nextPossible = potentialWords[nextPossiblei++];
            //if (nextPossiblei >= potentialWords.Count)
            //    nextPossiblei = 0;
            //if (nextPossible.Length == chainWord.Length)
            //{
            //    nextPossible = potentialWords[nextPossiblei++];
            //    if (nextPossiblei >= potentialWords.Count)
            //        nextPossiblei = 0;
            //}
            string randomNab = null;
            int randomStart = game1.rand.Next(potentialWords.Count);
            int minCommon = int.MaxValue;
            bool newLetterFound = false;
            for (int j = 0; j < potentialWords.Count; ++j)
            {
                int index = (randomStart + j) % potentialWords.Count;
                randomNab = potentialWords[index];
                int commonLevel = dictionary[randomNab[0]][randomNab.Length][randomNab].commonLevel;
                if (randomNab.Length != chainWord.Length)
                {
                    char letter = randomNab[chainWord.Length];
                    bool commonEnough = commonLevel < minCommon || (game1.rand.Next(10) == 0 && commonLevel == minCommon);
                    bool betterLetter = !newLetterFound && !existingLetters.Contains(letter);
                    bool asGoodLetter = newLetterFound && !existingLetters.Contains(letter);
                    bool goodLetter = betterLetter || asGoodLetter;
                    if (commonEnough && goodLetter)
                    {
                        if (betterLetter)
                            newLetterFound = true;
                        nextPossible = randomNab;
                        if (commonLevel >= targetCommonLevel)
                        {
                            minCommon = commonLevel;
                        }
                    }
                    //if (commonLevel == targetCommonLevel)
                    //{
                    //    break;
                    //}
                }
            }
            return nextPossible;
        }
        public bool PointInBounds(int x, int y)
        {
            if (x < 0 || x >= mapW || y < 0 || y >= mapH)
            {
                return false;
            }
            return true;
        }
        public bool invalidMove (int x, int y)
        {
            bool result = false;
            bool inBounds = PointInBounds(x,y);
            if (!inBounds)
            {
                result = true;
            }
            if (inBounds && boardTiles[x, y].consumed)
            {
                result = true;
            }
            return result;
        }

        private void OnPostTileCollected(object v, EventArgs eventArgs)
        {
            pickupSfx.Play();
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
            if (false)
            {
                requestReset = true;
                collectionDuration = 0;// game1.randf(0.25f);
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
        public void UpdateWordPotential(Dictionary<int, Dictionary<string, WordData>> charcountTable)
        {
            lastPotentialWordCount = potentialWords.Count;
            potentialWords.Clear();
            potentialWordTable.Clear();
            int s = game1.rand.Next(charcountTable.Keys.Count); //starting index
            for (int i = 0; i < charcountTable.Keys.Count; ++i) //use for loop to iterate
            {
                int k = (s + i) % charcountTable.Keys.Count; //index of length key
                int length = charcountTable.Keys.ElementAt(k); //integer key referring to a word's length
                if (length < chainWord.Length) //discard words shorter than the current
                    continue;
                Dictionary<string, WordData> words = charcountTable[length]; //potential words
                foreach (var word in words)
                {
                    bool match = true; //roots match
                    for (int j = 0; j < chainWord.Length; ++j)
                    {
                        if (word.Key[j] != chainWord[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        potentialWords.Add(word.Key); //a 100% match is still a potential word
                        if (word.Key.Length >= chainWord.Length+1)
                        {
                            char letter = word.Key[chainWord.Length];
                            if (!potentialWordTable.ContainsKey(letter))
                                potentialWordTable.Add(letter, new List<string>());
                            potentialWordTable[letter].Add(word.Key);
                        }
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

        private void updateGUI()
        {
            playRect = Split_Screen_Dungeon.Backpack.percentage(ViewportRect, 0, playRectY, 1, playRectHeight);
            if (playRect.Bottom != ViewportRect.Bottom)
                playRect.Height++;
            Vector2 prcenter = playRect.Center.ToVector2();
            tileH = Split_Screen_Dungeon.Backpack.percentageH(playRect, 1f / 13f);
            tileW = tileH;
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

            tileOffset = container.Location.ToVector2() + tileSize / 2;

            guiPointerReleaseTile = null; // reset gui pointer release tile
            if (pointerUp() && pointerUpOld())
                guiPointerTapTile = null;
            for (int x = 0; x < mapW; ++x)
            {
                for (int y = 0; y < mapH; ++y)
                {
                    Tile T = boardTiles[x, y];
                    T.position = tileOffset + new Vector2(x * tileW, y * tileH);
                    T.rect = game1.centeredRect(T.position, tileW, tileH);
                    if (pointerRelease() && T.rect.Contains(pointer()))
                    {
                        guiPointerReleaseTile = T;
                        guiPointerReleaseTileCoord.X = x;
                        guiPointerReleaseTileCoord.Y = y;
                    }
                    if (pointerTap() && T.rect.Contains(pointer()))
                    {
                        guiPointerTapTile = T;
                        guiPointerTapTileCoord.X = x;
                        guiPointerTapTileCoord.Y = y;
                    }
                }
            }
            overheadRect = Split_Screen_Dungeon.Backpack.percentage(ViewportRect, 0, 0, 1, 0.1f);
            multiplierRect = Split_Screen_Dungeon.Backpack.percentagef(overheadRect, 0, 0, 1f / 6f, 1);
            scoreRect = Split_Screen_Dungeon.Backpack.percentage(overheadRect, 5f / 6f, 0, 1f / 6f, 1);
            if (pointerTap() && multiplierRect.ContainsPoint(pointer()))
            {
                requestReset = true;
            }
            if (scoreRect.Contains(pointer()) && pointerTap())
            {
                drawReview = !drawReview;
            }
            middleRect = overheadRect;
            middleRect.Width -= scoreRect.Width;
            middleRect.Width -= multiplierRect.Width;
            middleRect.X += scoreRect.Width;
            returnBtn = Backpack.percentagef(middleRect, 0f, 0.25f, 0.2f, .5f);
            if (returnBtn.ContainsPoint(pointerRaw()) && pointerTap())
            {
                rwg.activeScreen = parentScreen;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (requestReset)
                return;

            base.Draw(gameTime, spriteBatch);


            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //game1.GraphicsDevice.Clear(Color.Magenta);
            //game1.clearColor = Color.Magenta;

            game1.drawSquare(ViewportRect, Color.LimeGreen, 0);

            game1.drawSquare(playRect, monochrome(0.2f), 0);

            // draw board
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
                        if (T.chain < chainColors.Length)
                            bg = chainColors[T.chain];
                        else
                            bg = Color.Magenta;
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
                    Vector2 pos = T.position;
                    Vector2 tileSize = new Vector2(tileW, tileH);
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
                        Rectangle ra = Split_Screen_Dungeon.Backpack.percentage(T.rect, 0.1f, .6f, 1, .4f);
                        game1.drawString(game1.defaultLargerFont, "" + T.value, ra, fg, new Vector2(0, 1), true);

                        //draw letter
                        Rectangle rb = Split_Screen_Dungeon.Backpack.percentage(T.rect, 0, 0, 1, 3f / 4f);
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

            game1.drawSquare(overheadRect, monochrome(0.5f), 0);
                Rectf multiplierCircleRect = multiplierRect;
                multiplierCircleRect.X -= multiplierCircleRect.Width;
                multiplierCircleRect.Y -= multiplierCircleRect.Height;
                multiplierCircleRect.Width *= 2;
                multiplierCircleRect.Height *= 2;
                game1.drawCircle(multiplierCircleRect, monochrome(0.8f));

            //draw multiplier
            if (drawMultiplierFlag)
            {
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
            }


            //draw score
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

            game1.drawSquare(returnBtn, monochrome(0.2f), 0);
            game1.drawStringf(game1.defaultLargerFont, "back", returnBtn, monochrome(1.0f), new Vector2(0.5f), true, 1);

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

            if (drawDiscoveredWordsFlag)
            {
                for (float i = 0; i < currentCombo; ++i)
                {
                    float f = (float)currentCombo;
                    Rectf r = Split_Screen_Dungeon.Backpack.percentagef(mrb, i / f, 0, 1 / f, 1);
                    //string text = i == 0 ? "DE" : i == 1 ? "DEV" : i == 2 ? "DEVISE" : "DEVISES";
                    string text = discoveredWords[discoveredWords.Count - 1 - (int)i];
                    Rectf textbox = game1.CalculateTextContainer(game1.defaultLargerFont, text, r, monochrome(1), new Vector2(0.5f), true);
                    game1.drawSquare(textbox, monochrome(0), 0);
                    game1.drawStringf(game1.defaultLargerFont, text, textbox, monochrome(1), new Vector2(0.5f), true);
                }
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

            gameMode.OnDraw();
            //Draw3DBoard.Draw(this);
            //{
            //    var bsc = this;
            //    var sb = game1.spriteBatch;
            //    Matrix view = Matrix.Identity;
            //    Matrix projection = Matrix.CreateOrthographic(bsc.ViewportRect.Width, bsc.ViewportRect.Height, 0.01f, 100);
            //    //game1.GraphicsDevice.Clear(Color.Black);
            //    game1.drawString("TEST", new Rectangle(0, 0, 200, 200), Color.Red);
            //    game1.GraphicsDevice.Clear(Color.Black);
            //    for (int x = 0; x < bsc.boardTiles.GetLength(0); ++x)
            //    {
            //        for (int y = 0; y < bsc.boardTiles.GetLength(1); ++y)
            //        {
            //            var tile = bsc.boardTiles[x, y];
            //            game1.DrawModel(
            //                game1.cubeModel,
            //                Matrix.CreateScale(tile.rect.Width, tile.rect.Height, 1) *
            //                Matrix.CreateTranslation(tile.position.X, tile.position.Y, -10),
            //                view, projection, Color.White);
            //        }
            //    }
            //    game1.cubeModel.Draw(Matrix.CreateScale(5) * Matrix.CreateTranslation(0, 0, -10), view, projection);
            //}

            if (drawReview)
            {
                Rectangle reviewRect = playRect;
                float border = Split_Screen_Dungeon.Backpack.percentageH(playRect, 1f / 32f) / 2;
                reviewRect.Inflate(-border, -border);
                float lightv = 0.8f;
                game1.drawSquare(reviewRect, monochrome(lightv, 0.8f), 0);
                // draw discovered words
                for (float i = 0; i < discoveredWords.Count + 2; ++i)
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
                        text = d + ". " + discoveredWords[d];
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

            //game1.GraphicsDevice.Clear(Color.Black);
        }
    }
}