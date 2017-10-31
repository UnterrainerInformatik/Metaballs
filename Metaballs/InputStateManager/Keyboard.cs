﻿// *************************************************************************** 
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
using Microsoft.Xna.Framework.Input;

namespace Metaballs.InputStateManager
{
    [PublicAPI]
    public class Keyboard
    {
        public KeyboardState OldKeyboardState { get; set; }
        public KeyboardState KeyboardState { get; set; }

        public bool IsDown(Keys key) => KeyboardState.IsKeyDown(key);
        public bool IsUp(Keys key) => KeyboardState.IsKeyUp(key);
        public bool IsPress(Keys key) => KeyboardState.IsKeyDown(key) && OldKeyboardState.IsKeyUp(key);
        public bool IsRelease(Keys key) => OldKeyboardState.IsKeyDown(key) && KeyboardState.IsKeyUp(key);

        public bool IsShiftDown => IsDown(Keys.LeftShift) || IsDown(Keys.RightShift);
        public bool IsCtrlDown => IsDown(Keys.LeftControl) || IsDown(Keys.RightControl);
        public bool IsAltDown => IsDown(Keys.LeftAlt) || IsDown(Keys.RightAlt);

        public void Update()
        {
            OldKeyboardState = KeyboardState;
            KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        }
    }
}