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

using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace Metaballs
{
    [PublicAPI]
    public class Matrices
    {
        public Matrix ViewToWorld { get; } = Matrix.CreateScale(.01f);
        public Matrix WorldToView { get; }

        public Vector2 View;
        public Point ViewInt => View.ToPoint();
        public Vector2 World;
        public Point WorldInt => World.ToPoint();

        public Matrices(Vector2 view)
        {
            WorldToView = Matrix.Invert(ViewToWorld);
            View = view;
            World = Vector2.Transform(View, ViewToWorld);
        }

        public float TransformViewToWorld(float v)
        {
            return v * .01f;
        }

        public float TransformWorldToView(float v)
        {
            return v * 100f;
        }

        public Vector2 TransformViewToWorld(Vector2 v)
        {
            return Vector2.Transform(v, ViewToWorld);
        }

        public Vector2 TransformViewToWorld(float x, float y)
        {
            return TransformViewToWorld(new Vector2(x, y));
        }

        public Vector2 TransformViewToWorld(Point p)
        {
            return TransformViewToWorld(p.ToVector2());
        }
        
        public Vector2 TransformWorldToView(Vector2 v)
        {
            return Vector2.Transform(v, WorldToView);
        }

        public Vector2 TransformWorldToView(float x, float y)
        {
            return TransformWorldToView(new Vector2(x, y));
        }

        public Vector2 TransformWorldToView(Point p)
        {
            return TransformWorldToView(p.ToVector2());
        }
    }
}