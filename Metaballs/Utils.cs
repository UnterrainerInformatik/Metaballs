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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metaballs
{
    public static class Utils
    {
        /// <summary>
        /// Creates a metaball texture.
        /// <p><b>Beware: Don't forget to dispose the texture at the end. Noone will take care of that for you.</b></p>
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="color">The color.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <returns></returns>
        public static Texture2D CreateMetaballTexture(int radius, Color color, GraphicsDevice graphicsDevice)
        {
            int length = radius * 2;
            Color[] colors = new Color[length * length];

            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    float distance = Vector2.Distance(Vector2.One,
                        new Vector2(x, y) / radius);
                    float alpha = Falloff(distance);

                    colors[y * length + x] = color;
                    colors[y * length + x].A =
                        (byte)MathHelper.Clamp(alpha * 256f + 0.5f, 0f, 255f);
                }
            }

            Texture2D tex = new Texture2D(graphicsDevice, length, length);
            tex.SetData(colors);
            return tex;
        }

        private static float Falloff(float r)
        {
            if (0 <= r && r <= 1 / 3F)
            {
                return 1F - 3F * r * r;
            }
            if (1 / 3F < r && r <= 1)
            {
                return 2 / 3F * (1 - r) * (1 - r);
            }
            return 0f;
        }
    }
}