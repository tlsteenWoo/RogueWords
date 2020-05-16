using Microsoft.Xna.Framework;
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
        public static void Draw(BoardScreenClassic bsc)
        {
            var game1 = bsc.game1;
            var sb = game1.spriteBatch;
            //game1.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.ClearOptions.DepthBuffer, Color.Lime, 1, 0);
            Matrix view = Matrix.Identity;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, bsc.ViewportRect.Width, 0, bsc.ViewportRect.Height, 0.01f, 100);
            //game1.GraphicsDevice.Clear(Color.Black);
            //game1.drawString("TEST", new Rectangle(0, 0, 200, 200), Color.Red);
            for (int x = 0; x < bsc.boardTiles.GetLength(0); ++x)
            {
                for (int y = 0; y < bsc.boardTiles.GetLength(1); ++y)
                {
                    var tile = bsc.boardTiles[x, y];
                    game1.DrawModel(
                        game1.cubeModel, 
                        Matrix.CreateScale(tile.rect.Width/2, tile.rect.Height/2, 1) *
                        Matrix.CreateTranslation(
                            tile.position.X,
                            tile.position.Y,
                            -10),
                        view, projection, Color.White);
                }
            }
            game1.cubeModel.Draw(Matrix.CreateScale(5) * Matrix.CreateTranslation(0,0,-6), view, projection);
        }
    }
}