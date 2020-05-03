using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MknGames.Split_Screen_Dungeon.Environment
{
    public class StarrySkyEnv
    {
        public Vector4[] stars;
        float w, d;
        GameMG game1;
        float sizemin, sizemax;
        int count;

        public StarrySkyEnv(GameMG game, float width, float depth, float sizemin, float sizemax, int count)
        {
            game1 = game;
            w = width;
            d = depth;
            this.sizemin = sizemin;
            this.sizemax = sizemax;
            this.count = count;
            stars = new Vector4[count];
            Reload();
        }

        public void Draw(Matrix view, Matrix projection)
        {
            for (int s = 0; s < stars.Length; ++s)
            {
                game1.DrawModel(
                    game1.sphereModel, 
                    Matrix.CreateScale(stars[s].W) *
                    Matrix.CreateTranslation(stars[s].X, stars[s].Y, stars[s].Z),
                    view,
                    projection,
                    game1.monochrome(1.0f), false);
            }
        }

        public void Reload()
        {
            for (int s = 0; s < stars.Length; ++s)
            {
                Func<float> a = () => { return game1.randf(0, MathHelper.TwoPi); }; //random angle
                Vector3 e = new Vector3(a() % MathHelper.Pi, a(), a()); //random euler rotation
                Vector3 c = new Vector3(w / 2, 0, d / 2); //map center
                float r = Math.Max(w, d) / 2 * (10.0f); //distance of star
                Vector3 p = c + Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(e.Y, e.X, e.Z)) * r; // random position
                stars[s] = new Vector4(p, game1.randf(sizemin, sizemax));
            }
        }
    }
}
