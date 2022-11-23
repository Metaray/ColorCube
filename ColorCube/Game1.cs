using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ColorCube
{
    public class Game1 : Game
    {
        private const int Width = 900, Height = 800;

        private GraphicsDeviceManager graphics;
        private Effect spatialColorEffect;
        private Matrix mWorld, mView, mProjection;

        private MouseState currentMouseState, lastMouseState;
        private KeyboardState currentKeyboardState, lastKeyboardState;

        private float vAngle = 0, hAngle = MathF.PI;
        private int backgroundSelect = 0;
        private ColorsDisplayMode colorsDisplayMode = ColorsDisplayMode.RGB;

        private VertexPositionNormalTexture[] outlineVerts = null;
        private Color[] imageColors = null;
        private VertexBuffer particlesVbuf = null;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Width,
                PreferredBackBufferHeight = Height,
                //GraphicsProfile = GraphicsProfile.HiDef,
                //PreferMultiSampling = true,
                //SynchronizeWithVerticalRetrace = false,
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 75);
        }

        protected override void Initialize()
        {
            lastMouseState = currentMouseState = Mouse.GetState();
            lastKeyboardState = currentKeyboardState = Keyboard.GetState();

            const float aspect = (float)Width / Height;

            //mProjection = Matrix.CreatePerspectiveFieldOfView(
            //    MathHelper.ToRadians(80f),
            //    ascpect,
            //    1f, 550f
            //);

            const float orthoSize = 400;
            mProjection = Matrix.CreateOrthographic(
                orthoSize * aspect, orthoSize,
                0, 550
            );

            Window.FileDrop += OnFileDrop;

            base.Initialize();
        }

        private void OnFileDrop(object sender, FileDropEventArgs e)
        {
            if (e.Files.Length > 0)
            {
                ScheduleLoadImage(e.Files[0]);
            }
        }

        private void ScheduleLoadImage(string path)
        {
            Task.Run(() =>
            {
                try
                {
                    Trace.WriteLine("Getting Colors");
                    imageColors = ColorUtils.ImageUniqueColors(path);

                    Trace.WriteLine($"Image has {imageColors.Length} colors");
                    UpdateDisplayMode(colorsDisplayMode);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            });
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            spatialColorEffect = Content.Load<Effect>("mainshader");

            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                ScheduleLoadImage(args[1]);
            }

            UpdateDisplayMode(colorsDisplayMode);
        }

        protected override void Update(GameTime gameTime)
        {
            lastMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (!IsActive) return;
            Trace.WriteLine($"Update {gameTime.ElapsedGameTime}");

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                //Debug.WriteLine("{0} {1}", vAngle, hAngle);
                const float SpeedRatio = MathF.PI / 300; // radians/pixel
                int dx = currentMouseState.X - lastMouseState.X;
                int dy = currentMouseState.Y - lastMouseState.Y;
                hAngle += MathF.Abs(vAngle) <= MathF.PI * 0.5f || MathF.Abs(vAngle) >= MathF.PI * 1.5f
                    ? dx * SpeedRatio 
                    : dx * -SpeedRatio;
                hAngle %= MathF.PI * 2;
                vAngle += dy * SpeedRatio;
                //vAngle %= MathF.PI * 2;
                vAngle = Math.Min(Math.Max(vAngle, -MathF.PI / 2), MathF.PI / 2);
            }

            if ((lastMouseState.RightButton == ButtonState.Released) &&
                (currentMouseState.RightButton == ButtonState.Pressed))
            {
                backgroundSelect = (backgroundSelect + 1) % 2;
            }

            if (currentKeyboardState.IsKeyDown(Keys.F1) && lastKeyboardState.IsKeyUp(Keys.F1))
            {
                if (colorsDisplayMode != ColorsDisplayMode.RGB)
                {
                    UpdateDisplayMode(ColorsDisplayMode.RGB);
                }
            }
            else if (currentKeyboardState.IsKeyDown(Keys.F2) && lastKeyboardState.IsKeyUp(Keys.F2))
            {
                if (colorsDisplayMode != ColorsDisplayMode.HSV)
                {
                    UpdateDisplayMode(ColorsDisplayMode.HSV);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive) return;
            Trace.WriteLine($"Draw {gameTime.ElapsedGameTime}");

            GraphicsDevice.Clear(backgroundSelect == 0 ? Color.Black : Color.White);

            // Camera looks into -Z (XY aligned with right+down, Z points to viewer, left hand rule)
            // 1) Center cube at 0,0,0
            // 2) Rotate horizontally
            // 3) Titlt (rotate) vertically
            mWorld = Matrix.CreateTranslation(-127.5f, -127.5f, -127.5f);
            mWorld *= Matrix.CreateRotationY(hAngle) * Matrix.CreateRotationX(vAngle);

            // 3) Place camera behind object
            mView = Matrix.CreateTranslation(0, 0, -300);

            spatialColorEffect.Parameters["WorldViewProjection"].SetValue(mWorld * mView * mProjection);
            spatialColorEffect.Parameters["InvScreenSize"].SetValue(new Vector2(1.0f / Width, 1.0f / Height));

            foreach (EffectPass pass in spatialColorEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                if (outlineVerts != null)
                {
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, outlineVerts, 0, outlineVerts.Length / 2);
                }

                if (particlesVbuf != null)
                {
                    GraphicsDevice.SetVertexBuffer(particlesVbuf);
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, particlesVbuf.VertexCount / 3);
                }
            }

            base.Draw(gameTime);
        }

        private void UpdateDisplayMode(ColorsDisplayMode newMode)
        {
            colorsDisplayMode = newMode;

            outlineVerts = newMode switch
            {
                ColorsDisplayMode.RGB => new PointCloudRgb().MakeOutline(),
                ColorsDisplayMode.HSV => new PointCloudHsv().MakeOutline(),
                _ => throw new NotImplementedException()
            };

            if (imageColors != null)
            {
                var particlesVerts = newMode switch
                {
                    ColorsDisplayMode.RGB => new PointCloudRgb().ColorsToVertexes(imageColors),
                    ColorsDisplayMode.HSV => new PointCloudHsv().ColorsToVertexes(imageColors),
                    _ => throw new NotImplementedException()
                };

                if (particlesVbuf == null || particlesVbuf.VertexCount != particlesVerts.Length)
                {
                    particlesVbuf = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, particlesVerts.Length, BufferUsage.WriteOnly);
                }
                particlesVbuf.SetData(particlesVerts);
            }
        }

        private enum ColorsDisplayMode
        {
            RGB, HSV
        }
    }
}
