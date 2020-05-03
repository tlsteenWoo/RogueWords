//using MknGames.Split_Screen_Dungeon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static MknGames.Split_Screen_Dungeon.Backpack;

namespace MknGames._2D
{
    public class DrawableGameComponentMG : DrawableGameComponent
    {
        public GameMG game1;
        //public CameraState camera = new CameraState();

        public SpriteBatch spriteBatch
        {
            get { return game1.spriteBatch; }
        }

        public Model cubeModel
        {
            get { return game1.cubeModel; }
        }

        public DrawableGameComponentMG(GameMG game) : base(game)
        {
            game1 = game;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //camera.Update(gameTime, GraphicsDevice.Viewport);
        }

        public Color monochrome(float value, float alpha = 1, float hue = 360, float saturation = 0)
        {
            return game1.monochrome(value, alpha, hue, saturation);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            //Vector3 youPos = new Vector3(0, 0, -5);
            //DrawCube(Matrix.CreateTranslation(youPos), 
            //    monochrome(1.0f));
        }

        public void DrawArrow(Vector2 a, Vector2 b)
        {
            Color color = monochrome(1.0f);
            game1.drawLine(a, b, color, 1);
            Vector2 dir = a - b;
            dir.Normalize();
            float angle = (float)Math.Atan2(dir.Y, dir.X);
            float arrowHeadLength = 5;
            float arrowTilt = MathHelper.ToRadians(45);
            game1.drawLine(b, b + game1.fromAngle(angle - arrowTilt) * arrowHeadLength, color, 1);
            game1.drawLine(b, b + game1.fromAngle(angle + arrowTilt) * arrowHeadLength, color, 1);
        }

        public void DrawTextBox(string text, Vector2 position, float width, float height)
        {
            DrawTextBox(text, position, width, height, 1);
        }

        public void DrawTextBox(string text, Vector2 position, float width, float height, float colorMod)
        {
            Rectangle rect = game1.centeredRect(position, width, height);
            DrawTextBox(text, rect, 1);
        }
        public void DrawTextBox(string text, Rectangle rect, float colorMod)
        {
            game1.drawSquare(rect, game1.monochrome(0.3f * colorMod), 0);
            game1.drawString(text, rect, monochrome(1.0f * colorMod));
        }

        public bool DoButton(string text, Vector2 position, float width, float height)
        {
            Rectangle rect = game1.centeredRect(position, width, height);
            float colorMod = 1;
            bool click = false;
            if(rect.Contains(game1.mouseCurrent.Position))
            {
                colorMod = 2;
                if(game1.mouseCurrent.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    colorMod = 0.5f;
                    if (game1.mouseOld.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                    {
                        click = true;
                    }
                }
            }
            DrawTextBox(text, rect, colorMod);
            return click;
        }

        //public void DrawCube(Matrix world, Color color)
        //{
        //    game1.DrawModel(game1.cubeModel,
        //        world,
        //        camera.view,
        //        camera.projection,
        //        color);
        //}
    }
}
