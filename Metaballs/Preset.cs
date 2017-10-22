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
    public class Preset
    {
        public Color Glow { get; set; }
        public Color GradientInner { get; set; }
        public Color GradientOuter { get; set; }
        public float GlowFactor { get; set; }
        public float MaxDistance { get; set; }
        public float ScalingFactor { get; set; }
        public int Size { get; set; }

        public static Preset Lava()
            =>
                new Preset
                {
                    Glow = Color.DarkRed,
                    GradientInner = Color.Yellow,
                    GradientOuter = Color.DarkRed,
                    GlowFactor = .8f,
                    MaxDistance = 0.6f,
                    ScalingFactor = 0.8f,
                    Size = 120
                };

        public static Preset Water()
            =>
                new Preset
                {
                    Glow = Color.MidnightBlue,
                    GradientInner = Color.DodgerBlue,
                    GradientOuter = Color.MidnightBlue,
                    GlowFactor = .3f,
                    MaxDistance = 0.7f,
                    ScalingFactor = 0.9f,
                    Size = 100
                };
    }
}