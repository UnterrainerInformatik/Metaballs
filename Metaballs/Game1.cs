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
using System.Text;
using Metaballs.InputStateManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Controllers;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using Utilities;
using Mouse = Metaballs.InputStateManager.Mouse;

namespace Metaballs
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public const int MIN_SCREEN_RESOLUTION_WIDTH = 1024;
        public const int MIN_SCREEN_RESOLUTION_HEIGHT = 768;
        public readonly Point Bounds = new Point(MIN_SCREEN_RESOLUTION_WIDTH, MIN_SCREEN_RESOLUTION_HEIGHT);

        private SpriteBatch spriteBatch;
        private SpriteFont font;

        private Texture2D metaballTexture;
        private readonly List<Metaball> metaballs = new List<Metaball>();
        private RenderTarget2D metaballTarget;
        private AlphaTestEffect alphaTest;
        private readonly Random rand = new Random();

        private bool isGravity;
        private Preset preset;
        private int numberOfMetaballs = 120;

        private readonly World world;

        private ParticleHydrodynamicsController controller;
        private FixedMouseJoint fixedMouseJoint;

        private readonly InputStateManager.InputStateManager input;

        public Game1()
        {
            var graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = MIN_SCREEN_RESOLUTION_WIDTH;
            graphics.PreferredBackBufferHeight = MIN_SCREEN_RESOLUTION_HEIGHT;
            graphics.IsFullScreen = false;
            graphics.PreparingDeviceSettings += PrepareDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;

            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Content.RootDirectory = "Content";

            world = new World(new Vector2(0f, 9.80665f));
            var w = graphics.PreferredBackBufferWidth;
            var h = graphics.PreferredBackBufferHeight;
            world.CreateEdge(new Vector2(0f, 0f), new Vector2(0f, h));
            world.CreateEdge(new Vector2(0f, h), new Vector2(w, h));
            world.CreateEdge(new Vector2(w, h), new Vector2(w, 0f));
            world.CreateEdge(new Vector2(w, 0f), new Vector2(0f, 0f));
            world.CreateCircle(30, 0.0005f, new Vector2(500f, 500f), BodyType.Dynamic);

            world.JointRemoved += JointRemoved;
            input = new InputStateManager.InputStateManager();
        }

        void PrepareDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected virtual void JointRemoved(World sender, Joint joint)
        {
            if (fixedMouseJoint == joint)
                fixedMouseJoint = null;
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            metaballTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);

            controller = new ParticleHydrodynamics2Controller(2.0f, 2048);
            world.Add(controller);

            base.Initialize();
        }

        private void InitializeMetaballs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                metaballs.Add(CreateMetaball());
            }
        }

        private Metaball CreateMetaball()
        {
            Metaball m = new Metaball
            {
                Position =
                    new Vector2(rand.Next(GraphicsDevice.Viewport.Width), rand.Next(GraphicsDevice.Viewport.Height)),
                Texture = metaballTexture
            };
            m.Initialize(rand.Next());
            return m;
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Arsenal");

            alphaTest = new AlphaTestEffect(GraphicsDevice);
            var viewport = GraphicsDevice.Viewport;
            alphaTest.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) *
                                   Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            alphaTest.ReferenceAlpha = 128;
            Reset(Preset.Lava());
        }

        private void RecreateTexture()
        {
            metaballTexture?.Dispose();
            metaballTexture = MetaballGeneratorUtils.CreateMetaballTexture(preset.Size,
                MetaballGeneratorUtils.CreateFalloffFunctionCircle(1f, 1f),
                MetaballGeneratorUtils.CreateFalloffFunctionCircle(preset.MaxDistance, preset.ScalingFactor),
                MetaballGeneratorUtils.CreateTwoColorFunction(preset.GradientOuter, preset.GradientInner),
                GraphicsDevice);
            foreach (var metaball in metaballs)
            {
                metaball.Texture = metaballTexture;
            }
        }

        private void Reset(Preset p)
        {
            preset = p;
            RecreateTexture();
            /*metaballTexture = Utils.CreateMetaballTexture(120, Utils.CreateFalloffFunctionCircle(1f, 1f),
                Utils.CreateFalloffFunctionCircle(0.6f, 0.8f), Utils.CreateSingleColorFunction(Color.White),
                GraphicsDevice);*/
            foreach (var metaball in metaballs)
            {
                metaball.Remove();
            }
            metaballs.Clear();
            if (world.Fluid.Particles.Count > 0)
                world.Fluid.Particles.Clear();
            numberOfMetaballs = 120;
            InitializeMetaballs(numberOfMetaballs);
        }

        private void AdjustNumberOfMetaballs()
        {
            while (numberOfMetaballs > metaballs.Count)
            {
                metaballs.Add(CreateMetaball());
            }

            while (numberOfMetaballs < metaballs.Count)
            {
                var m = metaballs[metaballs.Count - 1];
                metaballs.Remove(m);
                m.Remove();
            }
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            metaballTexture.Dispose();
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();
            HandleMouseInput();
            if (isGravity)
            {
                float timeStep = Math.Min((float) gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, 1f / 30f);
                world.Step(timeStep);
            }

            foreach (var metaball in metaballs)
            {
                metaball.Update(gameTime, Bounds, isGravity);
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            DrawMetaballs(metaballTarget);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            DrawMetaballsGlow(preset.GlowFactor);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, alphaTest);
            spriteBatch.Draw(metaballTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            DrawText();
            DrawShapes();
            base.Draw(gameTime);
        }

        private void DrawShapes()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (var body in world.BodyList)
            {
                foreach (var fixture in body.FixtureList)
                {
                    if (fixture.Shape.ShapeType == ShapeType.Edge)
                    {
                        EdgeShape edge = (EdgeShape) fixture.Shape;
                        spriteBatch.DrawLine(edge.Vertex1, edge.Vertex2, Color.White, 1f, 1f);
                    }
                    if (fixture.Shape.ShapeType == ShapeType.Circle)
                    {
                        CircleShape circle = (CircleShape) fixture.Shape;
                        spriteBatch.DrawCircle(circle.Position, circle.Radius, 30, Color.AntiqueWhite, 2f, 1f);
                    }
                }
            }
            spriteBatch.End();
        }

        private void DrawMetaballs(RenderTarget2D target)
        {
            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            if (isGravity)
            {
                var origin = new Vector2(metaballTexture.Width, metaballTexture.Height) / 2f;
                for (int i = 0; i < controller.ParticleCount; i++)
                    spriteBatch.Draw(metaballTexture, controller.GetParticlePosition(i) - origin, Color.White);
            }
            else
            {
                foreach (var metaball in metaballs)
                    spriteBatch.Draw(metaball.Texture, metaball.Position - metaball.Origin, Color.White);
            }
            spriteBatch.End();
        }

        /// <summary>
        ///     Draws a faint glow behind the metaballs. We accomplish this by rendering the metaball texture without threshholding
        ///     it. This is purely aesthetic.
        /// </summary>
        /// <param name="weight">The weight.</param>
        private void DrawMetaballsGlow(float weight)
        {
            Color tint = new Color(preset.Glow.ToVector3() * weight);
            var origin = new Vector2(metaballTexture.Width, metaballTexture.Height) / 2f;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            if (isGravity)
            {
                for (int i = 0; i < controller.ParticleCount; i++)
                    spriteBatch.Draw(metaballTexture, controller.GetParticlePosition(i) - origin, tint);
            }
            else
            {
                foreach (var metaball in metaballs)
                {
                    spriteBatch.Draw(metaball.Texture, metaball.Position - metaball.Origin, null, tint, 0f, Vector2.Zero,
                        1f, SpriteEffects.None, 0f);
                }
            }
            spriteBatch.End();
        }

        private void HandleInput()
        {
            input.Update();
            if (input.GamePad.IsPress(Buttons.Back) || input.Keyboard.IsPress(Keys.Escape))
                Exit();
            if (input.Keyboard.IsPress(Keys.R)) Reset(Preset.Lava());
            if (input.Keyboard.IsPress(Keys.T)) Reset(Preset.Water());
            if (input.Keyboard.IsPress(Keys.G))
            {
                isGravity = !isGravity;
                if (isGravity)
                {
                    world.Remove(controller);
                    controller = new ParticleHydrodynamics2Controller(2.0f, 2048);
                    world.Add(controller);
                    foreach (var metaball in metaballs)
                    {
                        controller.AddParticle(metaball.Position, metaball.Trajectory * metaball.Velocity);
                    }
                }
            }

            bool valModified = false;
            preset.GlowFactor = HandleFloatInput(Keys.Q, Keys.W, .1f, preset.GlowFactor, ref valModified).Clamp(0f, 1f);
            numberOfMetaballs = HandleIntInput(Keys.A, Keys.S, 1, numberOfMetaballs, ref valModified)
                .Clamp(0, int.MaxValue);

            bool textureModified = false;
            preset.Glow = HandleColorInput(Keys.NumPad7, Keys.NumPad8, Keys.NumPad9, preset.Glow, ref textureModified);
            preset.GradientInner = HandleColorInput(Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, preset.GradientInner,
                ref textureModified);
            preset.GradientOuter = HandleColorInput(Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, preset.GradientOuter,
                ref textureModified);
            preset.MaxDistance =
                HandleFloatInput(Keys.Y, Keys.X, .01f, preset.MaxDistance, ref textureModified, true).Clamp(0f, 1f);
            preset.ScalingFactor =
                HandleFloatInput(Keys.C, Keys.V, .01f, preset.ScalingFactor, ref textureModified, true).Clamp(0f, 1f);
            preset.Size = HandleIntInput(Keys.B, Keys.N, 1, preset.Size, ref textureModified, true)
                .Clamp(0, int.MaxValue);
            if (textureModified)
            {
                RecreateTexture();
            }
            AdjustNumberOfMetaballs();
        }

        private float HandleFloatInput(Keys down, Keys up, float step, float value, ref bool isModified,
            bool repeat = false)
        {
            float v = 0;
            if (!repeat && input.Keyboard.IsPress(up) || repeat && input.Keyboard.IsDown(up))
            {
                v = step;
                isModified = true;
            }
            if (!repeat && input.Keyboard.IsPress(down) || repeat && input.Keyboard.IsDown(down))
            {
                v = -step;
                isModified = true;
            }
            return value + v;
        }

        private int HandleIntInput(Keys down, Keys up, int step, int value, ref bool isModified, bool repeat = false)
        {
            int v = 0;
            if (!repeat && input.Keyboard.IsPress(up) || repeat && input.Keyboard.IsDown(up))
            {
                v = step;
                isModified = true;
            }
            if (!repeat && input.Keyboard.IsPress(down) || repeat && input.Keyboard.IsDown(down))
            {
                v = -step;
                isModified = true;
            }
            return value + v;
        }

        private Color HandleColorInput(Keys keyR, Keys keyG, Keys keyB, Color c, ref bool isModified)
        {
            Color color = c;
            int v = input.Keyboard.IsCtrlDown ? 1 : -1;
            if (input.Keyboard.IsDown(keyR))
            {
                color = new Color((color.R + v).Clamp(0, 255), color.G, color.B, color.A);
                isModified = true;
            }
            if (input.Keyboard.IsDown(keyG))
            {
                color = new Color(color.R, (color.G + v).Clamp(0, 255), color.B, color.A);
                isModified = true;
            }
            if (input.Keyboard.IsDown(keyB))
            {
                color = new Color(color.R, color.G, (color.B + v).Clamp(0, 255), color.A);
                isModified = true;
            }
            return color;
        }

        private void HandleMouseInput()
        {
            var position = input.Mouse.Position.ToVector2();
            if (input.Mouse.IsRelease(Mouse.Button.LEFT))
                MouseUp();
            else if (input.Mouse.IsPress(Mouse.Button.LEFT))
                MouseDown(position);

            MouseMove(position);
        }

        private void MouseDown(Vector2 p)
        {
            if (fixedMouseJoint != null)
                return;

            Fixture fixture = world.TestPoint(p);

            if (fixture != null)
            {
                Body body = fixture.Body;
                fixedMouseJoint = new FixedMouseJoint(body, p);
                fixedMouseJoint.MaxForce = 1000.0f * body.Mass;
                world.Add(fixedMouseJoint);
                body.Awake = true;
            }
        }

        private void MouseUp()
        {
            if (fixedMouseJoint != null)
            {
                world.Remove(fixedMouseJoint);
                fixedMouseJoint = null;
            }
        }

        private void MouseMove(Vector2 p)
        {
            if (fixedMouseJoint != null)
                fixedMouseJoint.WorldAnchorB = p;
        }

        private void DrawText()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Reset Scene: Lava(r), Water(t)\n");
            b.Append($"Gravity: {isGravity} (g)\n");
            b.Append($"Number of Metaballs: {numberOfMetaballs} <(a), >(s)\n");
            b.Append($"GlowFactor: {preset.GlowFactor:0.##} <(q), >(w)\n");
            b.Append($"GlowColor: {preset.Glow} <(num789), >(ctrl)(num789)\n");
            b.Append($"Texture_GradientInner: {preset.GradientInner} <(num456), >(ctrl)(num456)\n");
            b.Append($"Texture_GradientOuter: {preset.GradientOuter} <(num123), >(ctrl)(num123)\n");
            b.Append($"Texture_MaxDistance: {preset.MaxDistance:0.##} <(y), >(x)\n");
            b.Append($"Texture_ScalingFactor: {preset.ScalingFactor:0.##} <(c), >(v)\n");
            b.Append($"Texture_Size: {preset.Size} <(b), >(n)\n");

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, b, new Vector2(10, 10), Color.White);
            spriteBatch.End();
        }
    }
}