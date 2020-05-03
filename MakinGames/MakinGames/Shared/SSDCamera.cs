using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MknGames.Split_Screen_Dungeon
{
    public class CameraState : Camera
    {
        public Vector3 pos;
        public Vector3 Euler;
        public Matrix rotation3D;
        public float fov = 105;
        public float fov_min = 35;
        public float fov_max = 105;
        public float fov_rate = 200;
        public float near = 0.1f;
        public float far = 2000;
        public bool zooming = false;
        public bool ignoreEuler = false;

        public CameraState()
        {
            view = Matrix.Identity;
            projection = Matrix.Identity;
            rotation3D = Matrix.Identity;
        }

        public void Update(GameTime gameTime, Viewport viewport)
        {
            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (Euler.Y < 0)
                Euler.Y += MathHelper.TwoPi;
            while (Euler.Y > MathHelper.TwoPi)
                Euler.Y -= MathHelper.TwoPi;
            while (Euler.X < 0)
                Euler.X += MathHelper.TwoPi;
            while (Euler.X > MathHelper.TwoPi)
                Euler.X -= MathHelper.TwoPi;
            if (!ignoreEuler)
                rotation3D = Matrix.CreateFromYawPitchRoll(Euler.Y, Euler.X, 0);
            view = Matrix.CreateTranslation(-pos) *
                Matrix.Invert(rotation3D);
            projection = GetProjection(fov,
                viewport.AspectRatio);
            
            if (zooming)
            {
                fov -= et * fov_rate;
            }
            else
            {
                fov += et * fov_rate;
            }
            fov = MathHelper.Clamp(fov, fov_min, fov_max);
        }
        public Matrix GetProjection(float argFovDegrees, float aspectRatio)
        {
            return Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(argFovDegrees),
                aspectRatio,
                near, far);
        }

        public Vector3 ScreenToWorld(Vector2 screen, float depth, Viewport viewport)
        {
            return ScreenToWorld(screen, depth, viewport, view * projection);
        }
        public static Vector3 ScreenToWorld(Vector2 screen, float depth, Viewport viewport, Matrix transform)
        {
            Point toCenterRaw = screen.ToPoint() -
                viewport.Bounds.Center;
            Vector2 toCenter =
                toCenterRaw.ToVector2() /
                new Vector2(viewport.Width / 2, -viewport.Height / 2);
            Vector4 viewportNear = new Vector4(toCenter.X, toCenter.Y, depth, 1);
            Vector4 rawNear = Vector4.Transform(viewportNear, Matrix.Invert(transform));
            Vector3 near = new Vector3(rawNear.X, rawNear.Y, rawNear.Z) / rawNear.W;
            return near;
        }
        public Vector2 worldToScreen(Vector3 position, Viewport viewport)
        {
            Vector4 product = Vector4.Transform(new Vector4(position,1), view * projection);
            Vector2 viewPoint = new Vector2(product.X, product.Y) / product.W;
            viewPoint.Y *= -1;
            viewPoint *= 0.5f;
            viewPoint += new Vector2(0.5f);
            return viewPoint * viewport.Bounds.Size.ToVector2() + viewport.Bounds.Location.ToVector2();
        }
        public static Vector2 worldToScreen(Vector3 position, Viewport viewport, Matrix transform)
        {
            Vector4 product = Vector4.Transform(new Vector4(position, 1), transform);
            Vector2 viewPoint = new Vector2(product.X, product.Y) / product.W;
            viewPoint.Y *= -1;
            viewPoint *= 0.5f;
            viewPoint += new Vector2(0.5f);
            return viewPoint * viewport.Bounds.Size.ToVector2() + viewport.Bounds.Location.ToVector2();
        }
        public Ray ScreenToRay(Vector2 rawScreenPosition, Viewport viewport)
        {
            Vector3 near = ScreenToWorld(rawScreenPosition, 0, viewport);
            Vector3 far = ScreenToWorld(rawScreenPosition, 1, viewport);
            Ray ray = new Ray();
            ray.Position = near;
            ray.Direction = Vector3.Normalize(far - near);
            return ray;
        }
    }
}
