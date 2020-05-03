using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MknGames
{
    public class Camera
    {
        public Matrix view;
        public Matrix projection;

        /// <summary>
        /// (UNTESTED)
        /// </summary>
        Vector3 TransformDirection(Vector3 untransformedDirection)
        {
            Vector3 origin = Vector3.Transform(Vector3.Zero, view);
            Vector3 destination = Vector3.Transform(untransformedDirection, view);
            Vector3 result = destination - origin;
            result.Normalize();
            return result;
        }

        /// <summary>
        /// (UNTESTED)
        /// </summary>
        public Matrix FindOrientationMatrix()
        {
            return Matrix.CreateWorld(
                Vector3.Zero,
                TransformDirection(Vector3.Forward),
                TransformDirection(Vector3.Up)
                );
        }
    }
}