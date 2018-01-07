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
using InputStateManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameDemoTools;
using MonoGameDemoTools.Structures;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Controllers;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Fluids;
using tainicom.Aether.Physics2D.Maths;
using Utilities;
using Mouse = InputStateManager.Inputs.Mouse;

namespace Metaballs
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public const int MIN_SCREEN_RESOLUTION_WIDTH = 1024;
        public const int MIN_SCREEN_RESOLUTION_HEIGHT = 768;

        public const int ZX = 6;
        public const int ZY = 6;
        public const int W = MIN_SCREEN_RESOLUTION_WIDTH - 6;
        public const int H = MIN_SCREEN_RESOLUTION_HEIGHT - 6;

        public readonly RectangleF Bounds;

        private SpriteBatch spriteBatch;
        private SpriteFont font;

        private Texture2D metaballTexture;
        private readonly List<Metaball> metaballs = new List<Metaball>();
        private RenderTarget2D metaballTarget;
        private AlphaTestEffect alphaTest;
        private readonly Random rand = new Random();

        private bool isGravity;
        private bool isDebugDraw;
        private Preset preset;
        private int numberOfMetaballs = 120;

        private readonly World world;
        private ParticleHydrodynamicsController controller;
        private FixedMouseJoint fixedMouseJoint;

        private readonly InputManager input = new InputManager();
        private readonly Matrices m;

        public Game1()
        {
            m = new Matrices(new Vector2(MIN_SCREEN_RESOLUTION_WIDTH, MIN_SCREEN_RESOLUTION_HEIGHT));
            Bounds = new RectangleF(m.TransformViewToWorld(ZX, ZY), m.TransformViewToWorld(W, H));
            var graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = m.ViewInt.X;
            graphics.PreferredBackBufferHeight = m.ViewInt.Y;
            graphics.IsFullScreen = false;
            graphics.PreparingDeviceSettings += PrepareDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;

            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Content.RootDirectory = "Content";

            world = new World();
            world.Fluid = new FluidSystem2(new Vector2(0, -9.80665f), 5000, m.WorldInt.X, m.WorldInt.Y, 5);
            world.Gravity = new Vector2(0f, 9.80665f);
            world.CreateEdge(m.TransformViewToWorld(ZX, H), m.TransformViewToWorld(ZX, ZY));
            world.CreateEdge(m.TransformViewToWorld(W, H), m.TransformViewToWorld(ZX, H));
            world.CreateEdge(m.TransformViewToWorld(W, ZY), m.TransformViewToWorld(W, H));
            world.CreateEdge(m.TransformViewToWorld(ZX, ZY), m.TransformViewToWorld(W, ZY));
            world.CreateCircle(m.TransformViewToWorld(40), 0.0005f, m.TransformViewToWorld(500f, 500f), BodyType.Dynamic);

            world.JointRemoved += JointRemoved;
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
            Metaball b = new Metaball
            {
                Position = m.TransformViewToWorld(rand.Next(W + ZX), rand.Next(H + ZY)) - new Vector2(ZX, ZY),
                Texture = metaballTexture
            };
            b.Initialize(rand.Next(), Bounds, m);
            return b;
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("AnonymousPro8");

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
                var b = metaballs[metaballs.Count - 1];
                metaballs.Remove(b);
                b.Remove();
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
            FluidSettings s = world.Fluid.settings;
            HandleInput();
            HandleMouseInput();
            if (isGravity)
            {
                float timeStep = Math.Min((float) gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, 1f / 30f);
                world.Step(timeStep);
            }

            foreach (var metaball in metaballs)
            {
                if (isGravity)
                {
                    metaball.Position = controller.GetParticlePosition(metaball.ParticleIndex);
                }
                else
                    metaball.Update(gameTime, Bounds);
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            DrawMetaballs(metaballTarget, gameTime);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            DrawMetaballsGlow(preset.GlowFactor);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, alphaTest);
            spriteBatch.Draw(metaballTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            DrawText(gameTime);
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
                        spriteBatch.DrawLine(m.TransformWorldToView(edge.Vertex1), m.TransformWorldToView(edge.Vertex2),
                            Color.Gray, 2f, 1f);
                    }
                    if (fixture.Shape.ShapeType == ShapeType.Circle)
                    {
                        CircleShape circle = (CircleShape) fixture.Shape;
                        Vector2 p = m.TransformWorldToView(body.GetWorldPoint(circle.Position));
                        spriteBatch.DrawCircle(p, m.TransformWorldToView(circle.Radius), 30, Color.DarkSlateBlue, 2f, 1f);
                        Transform t;
                        body.GetTransform(out t);
                        spriteBatch.DrawLine(p,
                            p + m.TransformWorldToView(ComplexMultiply(new Vector2(1f, 0), ref t.q) * circle.Radius),
                            Color.DarkSlateBlue, 2f, 1f);
                    }
                }
            }
            spriteBatch.End();
        }

        public static Vector2 ComplexMultiply(Vector2 left, ref Complex right)
        {
            return new Vector2(left.X * right.Real - left.Y * right.Imaginary,
                left.Y * right.Real + left.X * right.Imaginary);
        }

        private void DrawMetaballs(RenderTarget2D target, GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            foreach (var metaball in metaballs)
                spriteBatch.Draw(metaball.Texture, m.TransformWorldToView(metaball.Position) - metaball.Origin,
                    Color.White);
            spriteBatch.End();

            if (isDebugDraw)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                foreach (var metaball in metaballs)
                    spriteBatch.DrawCircle(m.TransformWorldToView(metaball.Position), 5f, 4,
                        Demo.GetLerpColor(gameTime, Color.Red, Color.Black), 1f, 1f);
                spriteBatch.End();
            }
        }

        /// <summary>
        ///     Draws a faint glow behind the metaballs. We accomplish this by rendering the metaball texture without threshholding
        ///     it. This is purely aesthetic.
        /// </summary>
        /// <param name="weight">The weight.</param>
        private void DrawMetaballsGlow(float weight)
        {
            Color tint = new Color(preset.Glow.ToVector3() * weight);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            foreach (var metaball in metaballs)
                spriteBatch.Draw(metaball.Texture, m.TransformWorldToView(metaball.Position) - metaball.Origin, null,
                    tint, 0f, Vector2.Zero,
                    1f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        private void HandleInput()
        {
            input.Update();
            if (input.Pad.Is.Press(Buttons.Back) || input.Key.Is.Press(Keys.Escape))
                Exit();
            if (input.Key.Is.Press(Keys.R)) Reset(Preset.Lava());
            if (input.Key.Is.Press(Keys.T)) Reset(Preset.Water());
            if (input.Key.Is.Press(Keys.G))
            {
                isGravity = !isGravity;
                if (isGravity)
                {
                    world.Remove(controller);
                    controller = new ParticleHydrodynamics2Controller(2.0f, 2048);
                    world.Add(controller);
                    int i = 0;
                    foreach (var metaball in metaballs)
                    {
                        controller.AddParticle(metaball.Position, metaball.Trajectory * metaball.Velocity);
                        metaball.ParticleIndex = i;
                        i++;
                    }
                }
            }
            if (input.Key.Is.Press(Keys.H)) isDebugDraw = !isDebugDraw;

            bool valModified = false;
            preset.GlowFactor = input.FloatInput(Keys.Q, Keys.W, .1f, preset.GlowFactor, ref valModified).Clamp(0f, 1f);
            numberOfMetaballs = input.IntInput(Keys.A, Keys.S, 1, numberOfMetaballs, ref valModified, true)
                .Clamp(int.MaxValue);

            bool textureModified = false;
            preset.Glow = input.ColorInput(Keys.NumPad7, Keys.NumPad8, Keys.NumPad9, null, preset.Glow,
                ref textureModified);
            preset.GradientInner = input.ColorInput(Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, null, preset.GradientInner,
                ref textureModified);
            preset.GradientOuter = input.ColorInput(Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, null, preset.GradientOuter,
                ref textureModified);
            preset.MaxDistance =
                input.FloatInput(Keys.Y, Keys.X, .01f, preset.MaxDistance, ref textureModified, true).Clamp(1f);
            preset.ScalingFactor =
                input.FloatInput(Keys.C, Keys.V, .01f, preset.ScalingFactor, ref textureModified, true).Clamp(1f);
            preset.Size = input.IntInput(Keys.B, Keys.N, 10, preset.Size, ref textureModified, true)
                .Clamp(int.MaxValue);

            bool physModified = false;
            world.Fluid.settings.RestLength = input.FloatInput(Keys.D1, .01f, world.Fluid.settings.RestLength,
                ref physModified, true);
            world.Fluid.settings.InfluenceRadius = input.FloatInput(Keys.D2, .01f, world.Fluid.settings.InfluenceRadius,
                ref physModified, true);
            world.Fluid.settings.CollisionForce = input.FloatInput(Keys.D4, .01f, world.Fluid.settings.CollisionForce,
                ref physModified, true);
            world.Fluid.settings.DeformationFactor = input.FloatInput(Keys.D5, .01f,
                world.Fluid.settings.DeformationFactor,
                ref physModified, true);
            world.Fluid.settings.DensityRest = input.FloatInput(Keys.D6, .01f, world.Fluid.settings.DensityRest,
                ref physModified, true);
            world.Fluid.settings.KSpring = input.FloatInput(Keys.D7, .01f, world.Fluid.settings.KSpring,
                ref physModified, true);
            world.Fluid.settings.MaxNeighbors = input.IntInput(Keys.D8, 1, world.Fluid.settings.MaxNeighbors,
                ref physModified, true);
            world.Fluid.settings.Plasticity = input.FloatInput(Keys.D9, .01f, world.Fluid.settings.Plasticity,
                ref physModified, true);
            world.Fluid.settings.Stiffness = input.FloatInput(Keys.D0, .01f, world.Fluid.settings.Stiffness,
                ref physModified, true);
            world.Fluid.settings.StiffnessFarNearRatio = input.FloatInput(Keys.U, .01f,
                world.Fluid.settings.StiffnessFarNearRatio,
                ref physModified, true);
            world.Fluid.settings.VelocityCap = input.IntInput(Keys.I, 1, world.Fluid.settings.VelocityCap,
                ref physModified, true);
            world.Fluid.settings.ViscosityBeta = input.FloatInput(Keys.O, .01f, world.Fluid.settings.ViscosityBeta,
                ref physModified, true);
            world.Fluid.settings.ViscositySigma = input.FloatInput(Keys.J, .01f, world.Fluid.settings.ViscositySigma,
                ref physModified, true);
            world.Fluid.settings.YieldRatioCompress = input.FloatInput(Keys.K, .01f,
                world.Fluid.settings.YieldRatioCompress,
                ref physModified, true);
            world.Fluid.settings.YieldRatioStretch = input.FloatInput(Keys.L, .01f,
                world.Fluid.settings.YieldRatioStretch,
                ref physModified, true);

            if (textureModified)
            {
                RecreateTexture();
            }
            AdjustNumberOfMetaballs();
        }

        private void HandleMouseInput()
        {
            var position = input.Mouse.Is.Position.ToVector2();
            if (input.Mouse.Is.Release(Mouse.Button.LEFT))
                MouseUp();
            else if (input.Mouse.Is.Press(Mouse.Button.LEFT))
                MouseDown(position);

            MouseMove(position);
        }

        private void MouseDown(Vector2 p)
        {
            if (fixedMouseJoint != null)
                return;

            Fixture fixture = world.TestPoint(m.TransformViewToWorld(p));

            if (fixture != null)
            {
                Body body = fixture.Body;
                fixedMouseJoint = new FixedMouseJoint(body, m.TransformViewToWorld(p));
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
                fixedMouseJoint.WorldAnchorB = m.TransformViewToWorld(p);
        }

        private void DrawText(GameTime gameTime)
        {
            StringBuilder l = new StringBuilder();
            l.Append(" G E N E R A L:\n");
            l.Append("Reset Scene: Lava(r), Water(t)\n");
            l.Append($"Gravity: {isGravity} (g)\n");
            l.Append($"DebugDraw: {isDebugDraw} (h)\n");
            l.Append($"Number of Metaballs: {numberOfMetaballs} <(a), >(s)\n");
            l.Append($"GlowFactor: {preset.GlowFactor:0.##} <(q), >(w)\n");
            l.Append($"GlowColor: {preset.Glow} <(num789), >(ctrl/shft)(num789)\n");
            l.Append($"Texture_GradientInner: {preset.GradientInner} <(num456), >(ctrl/shft)(num456)\n");
            l.Append($"Texture_GradientOuter: {preset.GradientOuter} <(num123), >(ctrl/shft)(num123)\n");
            l.Append($"Texture_MaxDistance: {preset.MaxDistance:0.##} <(y), >(x)\n");
            l.Append($"Texture_ScalingFactor: {preset.ScalingFactor:0.##} <(c), >(v)\n");
            l.Append($"Texture_Size: {preset.Size} <(b), >(n)\n");

            FluidSettings s = world.Fluid.settings;
            StringBuilder r = new StringBuilder();
            r.Append("P H Y S I C S:\n");
            r.Append($"RestLength: {s.RestLength:0.###} <(1), >(ctrl/shft)(1)\n");
            r.Append($"InfluenceRadius: {s.InfluenceRadius:0.###} <(2), >(ctrl/shft)(2)\n");
            r.Append($"CollisionForce: {s.CollisionForce:0.###} <(4), >(ctrl/shft)(4)\n");
            r.Append($"DeformationFactor: {s.DeformationFactor:0.###} <(5), >(ctrl/shft)(5)\n");
            r.Append($"DensityRest: {s.DensityRest:0.###} <(6), >(ctrl/shft)(6)\n");
            r.Append($"KSpring: {s.KSpring:0.###} <(7), >(ctrl/shft)(7)\n");
            r.Append($"MaxNeighbors: {s.MaxNeighbors} <(8), >(ctrl/shft)(8)\n");
            r.Append($"Plasticity: {s.Plasticity:0.###} <(9), >(ctrl/shft)(9)\n");
            r.Append($"Stiffness: {s.Stiffness:0.###} <(0), >(ctrl/shft)(0)\n");
            r.Append($"StiffnessFarNearRatio: {s.StiffnessFarNearRatio:0.###} <(u), >(ctrl/shft)(u)\n");
            r.Append($"VelocityCap: {s.VelocityCap} <(i), >(ctrl/shft)(i)\n");
            r.Append($"ViscosityBeta: {s.ViscosityBeta:0.###} <(o), >(ctrl/shft)(o)\n");
            r.Append($"ViscositySigma: {s.ViscositySigma:0.###} <(j), >(ctrl/shft)(j)\n");
            r.Append($"YieldRatioCompress: {s.YieldRatioCompress:0.###} <(k), >(ctrl/shft)(k)\n");
            r.Append($"YieldRatioStretch: {s.YieldRatioStretch:0.###} <(l), >(ctrl/shft)(l)\n");

            Color c = Demo.GetLerpColor(gameTime);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, l, new Vector2(10, 10), c);
            spriteBatch.DrawString(font, r, new Vector2(10, 10) + new Vector2(m.ViewInt.X / 2f, 0f), c);
            spriteBatch.End();
        }
    }
}