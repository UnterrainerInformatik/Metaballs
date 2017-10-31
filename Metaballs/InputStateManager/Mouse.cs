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
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Metaballs.InputStateManager
{
    [PublicAPI]
    public class Mouse
    {
        public enum Button
        {
            LEFT,
            RIGHT,
            MIDDLE,
            X_BUTTON1,
            X_BUTTON2
        }

        public MouseState OldMouseState { get; set; }
        public MouseState MouseState { get; set; }

        public bool IsUp(Button button) => IsUp(MouseState, button);
        public bool IsDown(Button button) => IsDown(MouseState, button);
        public bool IsPress(Button button)
            => IsDown(MouseState, button) && IsUp(OldMouseState, button);
        public bool IsRelease(Button button)
            => IsDown(OldMouseState, button) && IsUp(MouseState, button);
        public Point Position => MouseState.Position;
        public int ScrollWheelValue => MouseState.ScrollWheelValue;
        public int HorizontalScrollWheelValue => MouseState.HorizontalScrollWheelValue;
        public int X => MouseState.X;
        public int Y => MouseState.Y;

        public bool IsOldUp(Button button) => IsUp(OldMouseState, button);
        public bool IsOldDown(Button button) => IsDown(OldMouseState, button);
        public Point OldPosition => OldMouseState.Position;
        public int OldScrollWheelValue => OldMouseState.ScrollWheelValue;
        public int OldHorizontalScrollWheelValue => OldMouseState.HorizontalScrollWheelValue;
        public int OldX => OldMouseState.X;
        public int OldY => OldMouseState.Y;

        public void Update()
        {
            OldMouseState = MouseState;
            MouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
        }

        private bool IsUp(MouseState state, Button button)
        {
            switch (button)
            {
                case Button.LEFT:
                    return state.LeftButton == ButtonState.Released;
                case Button.MIDDLE:
                    return state.MiddleButton == ButtonState.Released;
                case Button.RIGHT:
                    return state.RightButton == ButtonState.Released;
                case Button.X_BUTTON1:
                    return state.XButton1 == ButtonState.Released;
                case Button.X_BUTTON2:
                    return state.XButton2 == ButtonState.Released;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        private bool IsDown(MouseState state, Button button)
        {
            switch (button)
            {
                case Button.LEFT:
                    return state.LeftButton == ButtonState.Pressed;
                case Button.MIDDLE:
                    return state.MiddleButton == ButtonState.Pressed;
                case Button.RIGHT:
                    return state.RightButton == ButtonState.Pressed;
                case Button.X_BUTTON1:
                    return state.XButton1 == ButtonState.Pressed;
                case Button.X_BUTTON2:
                    return state.XButton2 == ButtonState.Pressed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }
    }
}