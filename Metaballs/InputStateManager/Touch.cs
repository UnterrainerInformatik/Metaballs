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
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Metaballs.InputStateManager
{
    [PublicAPI]
    public class Touch
    {
        private TouchLocation touchLocation;
        private TouchLocation oldTouchLocation;

        private Mouse mouse;
        private TouchCollection touchCollection;
        private List<GestureSample> gestures = new List<GestureSample>(4);
        private DateTime pressTimestamp;

        public TouchCollection TouchCollection => touchCollection;
        public TimeSpan MaxTapDuration = TimeSpan.FromMilliseconds(600);

        public bool IsGestureAvailable { get; set; }
        public bool IsMouseEmulation { get; set; } = true;

        public Touch(Mouse mouse)
        {
            this.mouse = mouse;
        }

        public void Update()
        {
            touchCollection = TouchPanel.GetState();

            IsGestureAvailable = false;
            if (TouchPanel.EnabledGestures != GestureType.None)
            {
                IsGestureAvailable = TouchPanel.IsGestureAvailable;
                gestures.Clear();
                while (TouchPanel.IsGestureAvailable)
                {
                    gestures.Add(TouchPanel.ReadGesture());
                }
            }

            if (IsMouseEmulation)
            {
                EmulateState();
                EmulateGestures();
            }
        }

        private void EmulateState()
        {
            Vector2 position = mouse.Position.ToVector2();
            oldTouchLocation = touchLocation;

            if (mouse.IsPress(Mouse.Button.LEFT))
                touchLocation = new TouchLocation(10000, TouchLocationState.Pressed, position);
            if (mouse.IsDown(Mouse.Button.LEFT) && mouse.IsOldDown(Mouse.Button.LEFT))
                touchLocation = new TouchLocation(touchLocation.Id, TouchLocationState.Moved,
                    position, touchLocation.State, touchLocation.Position);
            if (mouse.IsUp(Mouse.Button.LEFT) && mouse.IsOldDown(Mouse.Button.LEFT))
                touchLocation = new TouchLocation(touchLocation.Id, TouchLocationState.Released,
                    position, touchLocation.State, touchLocation.Position);
            if (mouse.IsUp(Mouse.Button.LEFT) && mouse.IsOldUp(Mouse.Button.LEFT))
                touchLocation = new TouchLocation();

            if (touchLocation.State != TouchLocationState.Invalid)
            {
                TouchLocation[] touchLocationArray = new TouchLocation[touchCollection.Count + 1];
                touchCollection.CopyTo(touchLocationArray, 0);
                touchLocationArray[touchLocationArray.Length - 1] = touchLocation;
                touchCollection = new TouchCollection(touchLocationArray);
            }
        }

        /// <summary>
        ///     Emulates tap, hold, moved, horizontalDrag, freeDrag and dragComplete only.
        /// </summary>
        private void EmulateGestures()
        {
            if (TouchPanel.EnabledGestures == GestureType.None) return;
            if (touchLocation.State == TouchLocationState.Invalid) return;

            Vector2 delta = touchLocation.Position - oldTouchLocation.Position;

            bool pressing = touchLocation.State == TouchLocationState.Pressed &&
                            (oldTouchLocation.State == TouchLocationState.Released ||
                             oldTouchLocation.State == TouchLocationState.Invalid);
            bool releasing = touchLocation.State == TouchLocationState.Released &&
                             (oldTouchLocation.State == TouchLocationState.Pressed ||
                              oldTouchLocation.State == TouchLocationState.Moved);

            if (pressing) pressTimestamp = DateTime.Now;
            TimeSpan pressDuration = DateTime.Now - pressTimestamp;

            if (releasing)
            {
                if ((TouchPanel.EnabledGestures & GestureType.Tap) != GestureType.None && delta == Vector2.Zero &&
                    pressDuration < MaxTapDuration)
                {
                    gestures.Add(new GestureSample(GestureType.Tap, TimeSpan.Zero,
                        touchLocation.Position, Vector2.Zero,
                        Vector2.Zero, Vector2.Zero));
                    IsGestureAvailable = true;
                }
            }

            if (touchLocation.State == TouchLocationState.Moved &&
                oldTouchLocation.State == TouchLocationState.Moved)
            {
                if ((TouchPanel.EnabledGestures & GestureType.Hold) != GestureType.None && delta == Vector2.Zero)
                {
                    gestures.Add(new GestureSample(GestureType.Hold, TimeSpan.Zero,
                        touchLocation.Position, Vector2.Zero,
                        Vector2.Zero, Vector2.Zero));
                    IsGestureAvailable = true;
                }
            }

            if (touchLocation.State == TouchLocationState.Moved)
            {
                if ((TouchPanel.EnabledGestures & GestureType.HorizontalDrag) != GestureType.None &&
                    !(Math.Abs(delta.X - 0f) < float.Epsilon))
                {
                    gestures.Add(new GestureSample(GestureType.HorizontalDrag, TimeSpan.Zero,
                        touchLocation.Position, Vector2.Zero,
                        delta, Vector2.Zero));
                    IsGestureAvailable = true;
                }
                if ((TouchPanel.EnabledGestures & GestureType.FreeDrag) != GestureType.None && delta != Vector2.Zero)
                {
                    gestures.Add(new GestureSample(GestureType.FreeDrag, TimeSpan.Zero,
                        touchLocation.Position, Vector2.Zero,
                        delta, Vector2.Zero));
                    IsGestureAvailable = true;
                }
            }

            if (touchLocation.State == TouchLocationState.Released &&
                oldTouchLocation.State == TouchLocationState.Moved)
            {
                if ((TouchPanel.EnabledGestures & GestureType.DragComplete) != GestureType.None)
                {
                    gestures.Add(new GestureSample(GestureType.DragComplete, TimeSpan.Zero,
                        touchLocation.Position, Vector2.Zero,
                        Vector2.Zero, Vector2.Zero));
                    IsGestureAvailable = true;
                }
            }
        }
    }
}