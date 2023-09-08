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
        private readonly GraphicsDeviceManager graphics;
        private Effect spatialColorEffect;
        private Matrix mWorld;
        private Matrix mView;
        private Matrix mProjection;

        private MouseState currentMouseState;
        private MouseState lastMouseState;
        private KeyboardState currentKeyboardState;
        private KeyboardState lastKeyboardState;

        private float vAngle = 0;
        private float hAngle = MathF.PI;
        private BackgroundStyle backgroundSelect = BackgroundStyle.Black;
        private ColorsDisplayMode colorsDisplayMode = ColorsDisplayMode.RGB;

        private VertexPositionNormalTexture[] outlineVerts = null;
        private Color[] imageColors = null;
        private VertexBuffer particlesVbuf = null;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 900,
                PreferredBackBufferHeight = 800,
                //GraphicsProfile = GraphicsProfile.HiDef,
                //PreferMultiSampling = true,
                //SynchronizeWithVerticalRetrace = false,
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += SizeChangedHandler;

            Window.FileDrop += OnFileDrop;
        }

        protected override void Initialize()
        {
            lastMouseState = currentMouseState = Mouse.GetState();
            lastKeyboardState = currentKeyboardState = Keyboard.GetState();

            mProjection = CreateProjectionMatrix();

            base.Initialize();
        }

        private Vector2 GetDrawBufferSize()
        {
            return new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        }

        private Matrix CreateProjectionMatrix()
        {
            Vector2 screenSize = GetDrawBufferSize();
            float aspect = screenSize.X / screenSize.Y;

            //return Matrix.CreatePerspectiveFieldOfView(
            //    MathHelper.ToRadians(80f),
            //    ascpect,
            //    1f, 550f
            //);

            const float orthoSize = 400;
            return Matrix.CreateOrthographic(
                orthoSize * aspect, orthoSize,
                0, 550
            );
        }

        private void SizeChangedHandler(object sender, EventArgs e)
        {
            mProjection = CreateProjectionMatrix();
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

            if (!IsActive)
            {
                return;
            }

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
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
                backgroundSelect = backgroundSelect == BackgroundStyle.Black ? BackgroundStyle.White : BackgroundStyle.Black;
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
            if (!IsActive)
            {
                return;
            }

            GraphicsDevice.Clear(backgroundSelect == BackgroundStyle.Black ? Color.Black : Color.White);

            // Camera looks into -Z (XY aligned with right+down, Z points to viewer, left hand rule)
            // 1) Center cube at 0,0,0
            // 2) Rotate horizontally
            // 3) Tilt (rotate) vertically
            mWorld = Matrix.CreateTranslation(-127.5f, -127.5f, -127.5f);
            mWorld *= Matrix.CreateRotationY(hAngle) * Matrix.CreateRotationX(vAngle);

            // 3) Place camera behind object
            mView = Matrix.CreateTranslation(0, 0, -300);

            spatialColorEffect.Parameters["WorldViewProjection"].SetValue(mWorld * mView * mProjection);
            spatialColorEffect.Parameters["InvScreenSize"].SetValue(Vector2.One / GetDrawBufferSize());

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
                    ColorsDisplayMode.RGB => new PointCloudRgb().ColorsToVertexData(imageColors),
                    ColorsDisplayMode.HSV => new PointCloudHsv().ColorsToVertexData(imageColors),
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
            RGB,
            HSV,
        }

        private enum BackgroundStyle
        {
            Black,
            White,
        }
    }
}
