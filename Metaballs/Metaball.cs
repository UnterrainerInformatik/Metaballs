// *************************************************************************** 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameDemoTools;
using MonoGameDemoTools.Structures;

namespace Metaballs
{
    public class Metaball
    {
        public Vector2 Position { get; set; }
        public Vector2 Origin => new Vector2(Texture.Width, Texture.Height) / 2f;
        public Vector2 Trajectory { get; set; }
        public float Velocity { get; set; }
        public int ParticleIndex { get; set; }

        public Texture2D Texture { get; set; }
        
        public void Initialize(int seed, RectangleF bounds, Matrices m)
        {
            Random rand = new Random(seed);
            Trajectory = new Vector2(rand.Next(-1000, 1000) / 1000f, rand.Next(-1000, 1000) / 1000f);
            Trajectory.Normalize();
            Position = new Vector2(rand.Next(0, 1000), rand.Next(0, 1000)) / 1000f;
            Position = bounds.TopLeft + bounds.WidthHeight * Position;
            Velocity = rand.Next(10, 400) / 100f;
            Velocity = m.TransformViewToWorld(Velocity);
        }

        public void Remove()
        {
            //World.Fluid.Particles.Remove(Particle);
        }
        
        public void Update(GameTime gameTime, RectangleF bounds)
        {
            Position = Position + Trajectory * Velocity;
            PositionTrajectory pt = Calculations.ConstrainAndReflect(Position, Trajectory, bounds);
            Position = pt.Position;
            Trajectory = pt.Trajectory;
        }
    }
}