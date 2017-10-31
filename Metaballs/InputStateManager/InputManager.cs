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
    public class InputManager
    {
        public enum MouseButton
        {
            LEFT,
            RIGHT,
            MIDDLE
        }

        public MouseState OldMouseState { get; set; }
        public MouseState MouseState { get; set; }

        public GamePadStates GamePadStates { get; set; } = new GamePadStates();

        public KeyboardState OldKeyboardState { get; set; }
        public KeyboardState KeyboardState { get; set; }

        public GamePadState OldGamePadState(PlayerIndex p = PlayerIndex.One) => GamePadStates.GetOld(p);
        public GamePadState GamePadState(PlayerIndex p = PlayerIndex.One) => GamePadStates.Get(p);

        public bool IsKeyDown(Keys key) => KeyboardState.IsKeyDown(key);
        public bool IsKeyUp(Keys key) => KeyboardState.IsKeyUp(key);
        public bool IsKeyPress(Keys key) => KeyboardState.IsKeyDown(key) && OldKeyboardState.IsKeyUp(key);
        public bool IsKeyRelease(Keys key) => OldKeyboardState.IsKeyDown(key) && KeyboardState.IsKeyUp(key);

        public bool IsShiftDown => IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
        public bool IsCtrlDown => IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);
        public bool IsAltDown => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);

        public bool IsConnected(PlayerIndex p = PlayerIndex.One) => GamePadState(p).IsConnected;
        public bool IsButtonDown(Buttons button, PlayerIndex p = PlayerIndex.One)
            => GamePadState(p).IsButtonDown(button);
        public bool IsButtonUp(Buttons button, PlayerIndex p = PlayerIndex.One)
            => GamePadState(p).IsButtonUp(button);
        public bool IsButtonPress(Buttons button, PlayerIndex p = PlayerIndex.One)
            => GamePadState(p).IsButtonDown(button) && OldGamePadState(p).IsButtonUp(button);
        public bool IsButtonRelease(Buttons button, PlayerIndex p = PlayerIndex.One)
            => OldGamePadState(p).IsButtonDown(button) && GamePadState(p).IsButtonUp(button);

        public bool IsMouseButtonUp(MouseButton button) => IsMouseButtonUp(MouseState, button);
        public bool IsMouseButtonDown(MouseButton button) => IsMouseButtonDown(MouseState, button);

        public bool IsMouseButtonPress(MouseButton button)
            => IsMouseButtonDown(MouseState, button) && IsMouseButtonUp(OldMouseState, button);

        public bool IsMouseButtonRelease(MouseButton button)
            => IsMouseButtonDown(OldMouseState, button) && IsMouseButtonUp(MouseState, button);

        private bool IsMouseButtonUp(MouseState state, MouseButton button)
        {
            switch (button)
            {
                case MouseButton.LEFT:
                    return state.LeftButton == ButtonState.Released;
                case MouseButton.MIDDLE:
                    return state.MiddleButton == ButtonState.Released;
                case MouseButton.RIGHT:
                    return state.RightButton == ButtonState.Released;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        private bool IsMouseButtonDown(MouseState state, MouseButton button)
        {
            switch (button)
            {
                case MouseButton.LEFT:
                    return state.LeftButton == ButtonState.Pressed;
                case MouseButton.MIDDLE:
                    return state.MiddleButton == ButtonState.Pressed;
                case MouseButton.RIGHT:
                    return state.RightButton == ButtonState.Pressed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        public void Update()
        {
            OldMouseState = MouseState;
            MouseState = Mouse.GetState();

            GamePadStates.Update();

            OldKeyboardState = KeyboardState;
            KeyboardState = Keyboard.GetState();
        }
    }
}