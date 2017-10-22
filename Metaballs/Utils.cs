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
        public delegate Vector3 ColorFunction(float alpha, float innerGradient);
        public delegate float FalloffFunction(float distance, int x, int y);

        /// <summary>
        /// Creates a metaball texture.
        /// <p><b>Beware: Don't forget to dispose the texture at the end. Noone will take care of that for you.</b></p>
        /// </summary>
        /// <param name="radius">Determines the distance at which the metaball has influence.</param>
        /// <param name="textureFalloff">The texture falloff.</param>
        /// <param name="colorFalloff">The color falloff.</param>
        /// <param name="colorFunction">A function that determines how to colour the metaball. Has no effect on their shape. It is purely
        /// aesthetic.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <returns>
        /// The texture used for making metabalss
        /// </returns>
        public static Texture2D CreateMetaballTexture(int radius, FalloffFunction textureFalloff, FalloffFunction colorFalloff, ColorFunction colorFunction, GraphicsDevice graphicsDevice)
        {
            int length = radius * 2;
            Color[] colors = new Color[length * length];

            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    float distance = Vector2.Distance(Vector2.One, new Vector2(x, y) / radius);

                    // This is the falloff function used to make the metaballs.
                    float alpha = textureFalloff(distance, x, y);

                    // We'll use a smaller, inner gradient to colour the center of the metaballs a different colour. This is purely aesthetic.
                    float innerGradient = colorFalloff(distance, x, y);
                    colors[y * length + x] = new Color(colorFunction(alpha, innerGradient));
                    colors[y * length + x].A = (byte) MathHelper.Clamp(alpha * 256f + 0.5f, 0f, 255f);
                }
            }

            Texture2D tex = new Texture2D(graphicsDevice, radius * 2, radius * 2);
            tex.SetData(colors);
            return tex;
        }

        /// <summary>
        ///     Colours the metaballs with a gradient between the two specified colours. For best result, the center colour
        ///     should be brighter than the border colour.
        /// </summary>
        public static ColorFunction CreateTwoColorFunction(Color border, Color center)
        {
            return (alpha, innerGradient) => Color.Lerp(border, center, innerGradient).ToVector3();
        }

        public static ColorFunction CreateSingleColorFunction(Color color)
        {
            return (alpha, innerGradient) => color.ToVector3();
        }

        /// <summary>
        ///     The falloff function for the metaballs.
        /// </summary>
        /// <param name="maxDistance">How far before the function goes to zero.</param>
        /// <param name="scalingFactor">Multiplies the function by this value.</param>
        /// <returns>The metaball value at the given distance.</returns>
        public static FalloffFunction CreateFalloffFunctionCircle(float maxDistance, float scalingFactor)
        {
            return (distance, x, y) => {
                if (distance <= maxDistance / 3)
                {
                    return scalingFactor * (1 - 3 * distance * distance / (maxDistance * maxDistance));
                }
                if (distance <= maxDistance)
                {
                    float x1 = 1 - distance / maxDistance;
                    return (3f / 2f) * scalingFactor * x1 * x1;
                }
                return 0;
            };
        }
    }
}