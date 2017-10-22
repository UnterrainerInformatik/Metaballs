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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        private Color glow;
        private float glowFactor;
        private int numberOfMetaballs = 120;

        private readonly InputManager input = new InputManager();

        public Game1()
        {
            var graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = MIN_SCREEN_RESOLUTION_WIDTH;
            graphics.PreferredBackBufferHeight = MIN_SCREEN_RESOLUTION_HEIGHT;
            graphics.IsFullScreen = false;
            graphics.PreparingDeviceSettings += PrepareDeviceSettings;
            Content.RootDirectory = "Content";
        }

        void PrepareDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
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

            metaballTexture = MetaballGeneratorUtils.CreateMetaballTexture(120,
                MetaballGeneratorUtils.CreateFalloffFunctionCircle(1f, 1f),
                MetaballGeneratorUtils.CreateFalloffFunctionCircle(0.6f, 0.8f),
                MetaballGeneratorUtils.CreateTwoColorFunction(Color.DarkRed, Color.Yellow),
                GraphicsDevice);
            /*metaballTexture = Utils.CreateMetaballTexture(120, Utils.CreateFalloffFunctionCircle(1f, 1f),
                Utils.CreateFalloffFunctionCircle(0.6f, 0.8f), Utils.CreateSingleColorFunction(Color.White),
                GraphicsDevice);*/
            alphaTest = new AlphaTestEffect(GraphicsDevice);
            var viewport = GraphicsDevice.Viewport;
            alphaTest.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) *
                                   Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            alphaTest.ReferenceAlpha = 128;

            Reset();
        }

        private void Reset()
        {
            metaballs.Clear();
            numberOfMetaballs = 120;
            InitializeMetaballs(numberOfMetaballs);
            glow = Color.Yellow;
            glowFactor = .3f;
        }

        private void AdjustNumberOfMetaballs()
        {
            while (numberOfMetaballs > metaballs.Count)
            {
                metaballs.Add(CreateMetaball()); 
            }

            while (numberOfMetaballs < metaballs.Count)
            {
                metaballs.RemoveAt(metaballs.Count - 1);
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

            foreach (var metaball in metaballs)
            {
                metaball.Update(gameTime);
                metaball.ConstrainAndReflect(Bounds);
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
            DrawMetaballsGlow(glowFactor);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, alphaTest);
            spriteBatch.Draw(metaballTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            DrawText();

            base.Draw(gameTime);
        }

        private void DrawMetaballs(RenderTarget2D target)
        {
            GraphicsDevice.SetRenderTarget(target);
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            foreach (var metaball in metaballs)
                spriteBatch.Draw(metaball.Texture, metaball.Position - metaball.Origin, Color.White);
            spriteBatch.End();
        }

        /// <summary>
        ///     Draws a faint glow behind the metaballs. We accomplish this by rendering the metaball texture without threshholding
        ///     it. This is purely aesthetic.
        /// </summary>
        /// <param name="weight">The weight.</param>
        private void DrawMetaballsGlow(float weight)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            foreach (var metaball in metaballs)
            {
                Color tint = new Color(glow.ToVector3() * weight);
                spriteBatch.Draw(metaball.Texture, metaball.Position - metaball.Origin, null, tint, 0f, Vector2.Zero, 1f,
                    SpriteEffects.None, 0f);
            }
            spriteBatch.End();
        }

        private void HandleInput()
        {
            input.Update();
            if (input.IsButtonPress(Buttons.Back) || input.IsKeyPress(Keys.Escape))
                Exit();
            if (input.IsKeyPress(Keys.R)) Reset();
            if (input.IsKeyPress(Keys.Q)) glowFactor = (glowFactor - .1f).Clamp(0f, 1f);
            if (input.IsKeyPress(Keys.W)) glowFactor = (glowFactor + .1f).Clamp(0f, 1f);
            if (input.IsKeyDown(Keys.A)) numberOfMetaballs = (numberOfMetaballs - 1).Clamp(0, int.MaxValue);
            if (input.IsKeyDown(Keys.S)) numberOfMetaballs = (numberOfMetaballs + 1).Clamp(0, int.MaxValue);
            AdjustNumberOfMetaballs();
        }

        private void DrawText()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Reset Scene: (r)\n");
            b.Append($"Number of Metaballs: {numberOfMetaballs} <(a), >(s)\n");
            b.Append($"GlowFactor: {glowFactor:0.##} <(q), >(w)\n");

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, b, new Vector2(10, 10), Color.White);
            spriteBatch.End();
        }
    }
}