using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MknGames
{
    public struct PhysicsState
    {
        public Vector3 pos, vel, force;
        public float mass;

        public PhysicsState(float mass)
        {
            this.mass = mass;
            pos = vel = force = Vector3.Zero;
        }

        public void Advance(float et)
        {
            Advance(et, ref pos, ref vel, ref force, mass);
        }

        public static void Advance(float et, ref Vector3 pos, ref Vector3 vel, ref Vector3 force, float mass = 1)
        {
            if (et == 0)
                return;
            vel += (force / mass) * et;
            force = Vector3.Zero;
            pos += vel * et;
        }
    }
}
