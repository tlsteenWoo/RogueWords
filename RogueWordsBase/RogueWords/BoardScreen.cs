using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MknGames.Rogue_Words
{
    public class BoardScreen : RogueWordsScreen
    {
        public class Tile
        {
            public char letter;
            public Vector2 position;

            public Tile(char letter, Vector2 position)
            {
                this.letter = letter;
                this.position = position;
            }
        }
        public class Slot
        {
            public Vector2 position;

            public Slot()
            {
            }
            public Slot(Vector2 position)
            {
                this.position = position;
            }
        }
        int boardWidth = 10;
        int boardHeight = 10;
        float tileWidth = 32;
        float tileHeight = 32;
        //Tile[,] tiles;
        //int cursorX = 0, cursorY = 0;
        //List<Point> selectedTiles = new List<Point>();
        //List<string> words = new List<string>();
        Tile[] tiles = new Tile[25];
        Tile grabTile;
        Slot[,] boardSlots;
        Slot holdSlot;
        Slot nextSlot;
        Slot discardSlot;
        //Slot[] discardSlots;
        Vector2 offset;
        float border = 10;
        List<Slot> allSlots = new List<Slot>();
        List<Tile> allTiles = new List<Tile>();

        public BoardScreen(RogueWordsGame Game) : base(Game)
        {
            //tiles = new Tile[boardWidth, boardHeight];
            //reset();
            //cursorX = boardWidth / 2;
            //cursorY = boardHeight / 2;boardSlots = new Slot[boardWidth, boardHeight];
            boardSlots = new Slot[boardWidth, boardHeight];
            //discardSlots = new Slot[tiles.Length];
        }
        void reset()
        {
            allTiles.Clear();
            for (int t = 0; t < tiles.Length;++t)
            {
                tiles[t] = new Tile(randChar(), 
                    new Vector2(holdSlot.position.X,
                    holdSlot.position.Y + tileHeight + border + t * 3)
                    );
                allTiles.Add(tiles[t]);
            }
            //for (int x = 0; x < boardWidth; ++x)
            //{
            //    for (int y = 0; y < boardHeight; ++y)
            //    {
            //        tiles[x, y] = new Tile();
            //        tiles[x, y].letter = randChar();
            //    }
            //}
        }
        public override void LoadContent()
        {
            base.LoadContent();
            Vector2 tileCenter = new Vector2(tileWidth, tileHeight) / 2;
            Vector2 boardCenter = new Vector2(tileWidth * boardWidth,
                tileHeight * boardHeight) / 2 - tileCenter;
            Vector2 screenCenter = game1.GraphicsDevice.Viewport.Bounds.Center.ToVector2();
            offset = screenCenter - boardCenter;
            offset.X = tileWidth / 2 + border;
            offset.Y = tileHeight;

            nextSlot = new Slot(new Vector2(
                game1.GraphicsDevice.Viewport.Bounds.Right - tileWidth/2 - border,
                offset.Y));
            allSlots.Add(nextSlot);

            holdSlot = new Slot(nextSlot.position + new Vector2(0, border + tileHeight));
            allSlots.Add(holdSlot);

            Vector2 lastPosition = offset;
            for (int x = 0; x < boardWidth; ++x)
            {
                for (int y = 0; y < boardHeight; ++y)
                {
                    //Tile tile = tiles[x, y];
                    if (boardSlots[x, y] == null)
                    {
                        lastPosition = offset + new Vector2(x * (tileWidth+2), y * (tileHeight+2));
                        boardSlots[x, y] = new Slot(lastPosition);
                        allSlots.Add(boardSlots[x, y]);
                    }
                }
            }

            Vector2 discardPosition = lastPosition + new Vector2(0, tileHeight + border);
            discardSlot = new Slot(discardPosition);
            allSlots.Add(discardSlot);
            //for(int i = 0; i < discardSlots.Length;++i)
            //{
            //    if (discardPosition.X > GameMG.GraphicsDevice.Viewport.Bounds.Right - tileWidth / 2)
            //    {
            //        discardPosition.X = offset.X;
            //        discardPosition.Y += tileHeight;
            //    }
            //    discardSlots[i] = new Slot(discardPosition);
            //    discardPosition.X += tileWidth;
            //}
            reset();
        }

        public override void Update(GameTime gameTime, float et)
        {
            base.Update(gameTime, et);
            //if (GameMG.kclick(Keys.Back))
            //{
            //    game.activeScreen = game.menuScreen;
            //    return;
            //}
            if(game1.kclick(Keys.R) || game1.kheld(Keys.R))
            {
                reset();
            }
            bool tileJustGrabbed = false;
            if (grabTile == null)
            {
                foreach (Tile t in tiles)
                {
                    Rectangle rect = game1.centeredRect(t.position, tileWidth, tileHeight);
                    if (rect.Contains(game1.mouseCurrent.Position))
                    {
                        if (game1.mouseCurrent.LeftButton == ButtonState.Pressed &&
                            game1.mouseOld.LeftButton == ButtonState.Released)
                        {
                            grabTile = t;
                            tileJustGrabbed = true;
                        }
                    }
                }
            }
            if(grabTile != null)
            {
                if (game1.mouseCurrent.LeftButton == ButtonState.Released)
                {
                    grabTile = null;
                }
                else
                {
                    grabTile.position = game1.mouseCurrent.Position.ToVector2();
                    if(tileJustGrabbed)
                    {
                        Tile[] reordered = new Tile[tiles.Length];
                        int count = 0;
                        foreach(Tile t in tiles)
                        {
                            if (t == grabTile) continue;
                            reordered[count++] = t;
                        }
                        reordered[count] = grabTile;
                        tiles = reordered;
                    }
                }
            }
            Dictionary<Slot, List<Tile>> slotData = new Dictionary<Slot, List<Tile>>();
            foreach(Tile t in tiles)
            {
                foreach(Slot s in allSlots)
                {
                    Vector2 toSlot = s.position - t.position;
                    float toSlotDist = toSlot.Length();
                    if(toSlotDist < tileWidth/4)
                    {
                        if (!slotData.ContainsKey(s))
                            slotData.Add(s, new List<Tile>());
                        t.position += toSlot + new Vector2(0, slotData[s].Count * 2);
                        slotData[s].Add(t);
                    }
                }
            }
            //Point oldCursor = new Point(cursorX, cursorY);
            //if (GameMG.kclick(Keys.Down))
            //{
            //    cursorY++;
            //}
            //if (GameMG.kclick(Keys.Up))
            //{
            //    cursorY--;
            //}
            //if (GameMG.kclick(Keys.Left))
            //{
            //    cursorX--;
            //}
            //if (GameMG.kclick(Keys.Right))
            //{
            //    cursorX++;
            //}
            //if (oldCursor != new Point(cursorX, cursorY))
            //{
            //    selectedTiles.Add(oldCursor);
            //}else if(GameMG.kclick(Keys.Enter))
            //{
            //    string word = "";
            //    for(int c = 0; c < selectedTiles.Count; ++c)
            //    {
            //        Tile t = tiles[selectedTiles[c].X, selectedTiles[c].Y];
            //        word += t.letter;
            //        t.letter = randChar();
            //    }
            //    selectedTiles.Clear();
            //    words.Add(word);
            //}
        }
        char randChar()
        {
            return (char)((int)'a' + game1.rand.Next(26));
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            //BACKGROUND
            game1.drawSquare(game1.GraphicsDevice.Viewport.Bounds, game1.monochrome(0.4f), 0);

            //BOARD
            foreach(Slot s in boardSlots)
            {
                DrawSlot(s);
            }
            DrawSlot(holdSlot);
            DrawSlot(nextSlot);
            DrawSlot(discardSlot);
            //foreach(Slot s in discardSlots)
            //{
            //    DrawSlot(s);
            //}

            //TILES
            foreach (Tile t in tiles)
            {
                rwg.game1.drawSquare(t.position,
                    Color.White, 0, tileWidth, tileHeight);
                rwg.game1.drawFrame(t.position,
                    Color.Black, tileWidth, tileHeight, 1);
                //spriteBatch.DrawString(game.game1.defaultFont, t.letter.ToString(),
                //    t.position, Color.Black);
                game1.drawString(t.letter.ToString(), game1.centeredRect(t.position, tileWidth, tileHeight), Color.Black,
                    new Vector2(0.5f,0.5f));
            }

            //CURSOR
            Color borderColor = game1.monochrome(1.0f, 0.5f);
            game1.drawFrame(game1.mouseCurrent.Position.ToVector2(), Color.Black,
                10, 10, 1);
            game1.drawFrame(game1.mouseCurrent.Position.ToVector2(), borderColor,
                8, 8, 1);
            Color centerColor = game1.monochrome(1.0f, 0.9f);
            game1.drawSquare(game1.mouseCurrent.Position.ToVector2(), Color.Black,
                0, 4, 4);
            game1.drawSquare(game1.mouseCurrent.Position.ToVector2(), centerColor,
                0, 2, 2);

            //for(int w = 0; w < words.Count; ++w)
            //{
            //    for(int c = 0; c < words[w].Length;++c)
            //    {
            //        Vector2 position = offset + new Vector2(c * tileWidth, (boardHeight+0.5f+w) * tileHeight);
            //        game.game1.drawFrame(position,
            //            Color.White, tileWidth, tileHeight, 1);
            //        spriteBatch.DrawString(game.game1.defaultFont, words[w][c].ToString(),
            //            position, Color.White);
            //    }
            //}
        }

        void DrawSlot(Slot s)
        {
            rwg.game1.drawSquare(s.position, game1.monochrome(0.3f), 0, tileWidth, tileHeight);
            Color slotFrameColor = game1.monochrome(0.2f);
            rwg.game1.drawFrame(s.position,
                slotFrameColor, tileWidth, tileHeight, 1);
        }
    }
}
