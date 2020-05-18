using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MknGames;
using MknGames.Rogue_Words;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueWordsBase.RogueWords
{
    public class Draw3DBoard
    {
        static RasterizerState backrs = new RasterizerState() { CullMode = CullMode.CullClockwiseFace };
        static RasterizerState frontrs = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace };
        public static void Draw(BoardScreenClassic bsc, GameTime gameTime)
        {
            var game1 = bsc.game1;
            var sb = game1.spriteBatch;

            bsc.game1.spriteBatch.Begin();
            for (int x = 0; x < bsc.boardTiles.GetLength(0); ++x)
            {
                for (int y = 0; y < bsc.boardTiles.GetLength(1); ++y)
                {
                    Tile T = bsc.boardTiles[x, y];
                    Rectangle copy = T.rect;
                    copy.Inflate(1, 1);
                    game1.drawSquare(copy, Color.Black, 0);
                }
            }
            sb.End();

                    //game1.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.ClearOptions.DepthBuffer, Color.Lime, 1, 0);
                    Matrix view = Matrix.Identity;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, bsc.ViewportRect.Width, 0, bsc.ViewportRect.Height, 0.01f, 1000);
            //game1.GraphicsDevice.Clear(Color.Black);
            //game1.drawString("TEST", new Rectangle(0, 0, 200, 200), Color.Red);
            for (int i = 0; i < 1; ++i)
            {
                if(i==0)
                    game1.GraphicsDevice.RasterizerState = frontrs;
                else
                game1.GraphicsDevice.RasterizerState = backrs;
                for (int x = 0; x < bsc.boardTiles.GetLength(0); ++x)
                {
                    for (int y = 0; y < bsc.boardTiles.GetLength(1); ++y)
                    {
                        Tile T = bsc.boardTiles[x, y];
                        Color bg = bsc.monochrome(0.2f);
                        Color fg = bsc.monochrome(0.5f);
                        bool drawInfo = false;
                        bool drawRoundedSquare = false;
                        bool drawFrame = false;
                        bool drawBackground = false;
                        bool lightingEnabled = true;
                        if (T.chain > -1)
                        {
                            if (T.chain < bsc.chainColors.Length)
                                bg = bsc.chainColors[T.chain];
                            else
                                bg = Color.Magenta;
                            fg = bsc.monochrome(1);
                            drawInfo = true;
                            drawFrame = true;
                            drawBackground = true;
                        }
                        else
                        {
                            if (T.consumed || T.visible)
                            {
                                drawInfo = true;
                                drawBackground = true;
                                if (T.consumed)
                                {
                                    drawFrame = true;
                                    drawInfo = false;
                                }
                                else
                                    drawRoundedSquare = true;
                            }
                            if (T.visible && !T.consumed)
                            {
                                bg = bsc.monochrome(0.5f);
                                fg = bsc.monochrome(1.0f);
                            }
                        }
                        bg.A = 150;
                        if (drawBackground)
                        {
                            game1.DrawModel(
                                bsc.roundedTileModel,
                                Matrix.CreateScale(T.rect.Width / 2, T.rect.Height / 2, 100) *
                                Matrix.CreateTranslation(
                                    T.position.X,
                                    bsc.ViewportRect.Height - T.position.Y,
                                    -150),
                                view, projection, bg, lightingEnabled);
                        }
                    }
                }
            }

            //int mode = (int)(gameTime.TotalGameTime.TotalSeconds % 10 / 5);
            //for (int i = 0; i < 2; ++i)
            //{
            //    if (i == 0)
            //        game1.GraphicsDevice.RasterizerState = frontrs;
            //    else
            //        game1.GraphicsDevice.RasterizerState = backrs;
            //    if ((mode == 0 && i == 0) ||
            //        (mode == 1 && i == 1) ||
            //        (mode == 2))
            //    {
            //        bsc.game1.DrawModel(bsc.roundedTileModel,
            //        Matrix.CreateScale(400, 400, 100) *
            //        Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds) *
            //        Matrix.CreateTranslation(bsc.ViewportRect.Width / 2, bsc.ViewportRect.Height / 2, -500),
            //        view,
            //        projection, new Color(1.0f, 0, 0, 0.7f), true);
            //    }
            //}

            bsc.game1.spriteBatch.Begin();
            for (int x = 0; x < bsc.boardTiles.GetLength(0); ++x)
            {
                for (int y = 0; y < bsc.boardTiles.GetLength(1); ++y)
                {
                    Tile T = bsc.boardTiles[x, y];
                    Color bg = bsc.monochrome(0);
                    Color fg = bsc.monochrome(0.5f);
                    bool drawInfo = false;
                    bool drawRoundedSquare = false;
                    bool drawFrame = false;
                    if (T.chain > -1)
                    {
                        if (T.chain < bsc.chainColors.Length)
                            bg = bsc.chainColors[T.chain];
                        else
                            bg = Color.Magenta;
                        fg = bsc.monochrome(1);
                        drawInfo = true;
                        drawFrame = true;
                    }
                    else
                    {
                        if (T.consumed || T.visible)
                        {
                            drawInfo = true;
                            if (T.consumed)
                            {
                                drawFrame = true;
                                drawInfo = false;
                            }
                            else
                                drawRoundedSquare = true;
                        }
                        if (T.visible && !T.consumed)
                        {
                            bg = bsc.monochrome(0.5f);
                            fg = bsc.monochrome(1.0f);
                        }
                    }
                    if (drawInfo)
                    {
                        //draw value
                        Rectangle ra = MknGames.Split_Screen_Dungeon.Backpack.percentage(T.rect, 0.1f, .6f, 1, .4f);
                        game1.drawString(game1.defaultLargerFont, "" + T.value, ra, fg, new Vector2(0, 1), true);

                        //draw letter
                        Rectangle rb = MknGames.Split_Screen_Dungeon.Backpack.percentage(T.rect, 0, 0, 1, 3f / 4f);
                        char letter = T.letter;
                        //if (T != chainTiles[0])
                        //    letter = char.ToLower(letter);
                        game1.drawString(game1.defaultLargerFont, "" + letter, rb.Size.ToVector2(), rb.Location.ToVector2(), fg, new Vector2(0.5f), true);
                    }
                }
            }
                    bsc.game1.spriteBatch.End();
        }

        public static Vector2 GetPosition(Vector2 pos, BoardScreenClassic bsc)
        {
            return new Vector2(pos.X, bsc.ViewportRect.Height - pos.Y);
        }
    }
}