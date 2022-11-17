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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            spatialColorEffect = Content.Load<Effect>("mainshader");

            string filePath = "test.png";
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                filePath = args[1];
            }

            Task.Run(() =>
            {
                Console.WriteLine("Getting Colors");
                if (System.IO.File.Exists(filePath))
                {
                    imageColors = ColorUtils.ImageUniqueColors(filePath);
                    Console.WriteLine("Found {0} colors", imageColors.Length);
                    UpdateDisplayMode(ColorsDisplayMode.RGB);
                }
                else
                {
                    Console.WriteLine("File \"{0}\" doesn't exist", filePath);
                }
            });
            outlineVerts = new PointCloudRgb().MakeOutline();
        }

        protected override void Update(GameTime gameTime)
        {
            lastMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (!IsActive) return;

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
                UpdateDisplayMode(ColorsDisplayMode.RGB);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.F2) && lastKeyboardState.IsKeyUp(Keys.F2))
            {
                UpdateDisplayMode(ColorsDisplayMode.HSV);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive) return;
            //Debug.WriteLine("{0}", gameTime.ElapsedGameTime);
            GraphicsDevice.Clear(backgroundSelect == 0 ? Color.Black : Color.White);

            // Camera looks into -Z (XY aligned with right+down, Z points to viewer, left hand rule)
            // 1) Center cube at 0,0,0
            // 2) Rotate horizontally
            // 3) Titlt (rotate) vertically
            mWorld = Matrix.CreateTranslation(-127.5f, -127.5f, -127.5f);
            mWorld *= Matrix.CreateRotationY(hAngle) * Matrix.CreateRotationX(vAngle);

            // 3) Place camera behind object
            mView = Matrix.CreateTranslation(0, 0, -300);

            spatialColorEffect.Parameters["World"].SetValue(mWorld);
            spatialColorEffect.Parameters["View"].SetValue(mView);
            spatialColorEffect.Parameters["Projection"].SetValue(mProjection);
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
            if (imageColors != null && (particlesVbuf == null || newMode != colorsDisplayMode))
            {
                colorsDisplayMode = newMode;

                outlineVerts = newMode switch
                {
                    ColorsDisplayMode.RGB => new PointCloudRgb().MakeOutline(),
                    ColorsDisplayMode.HSV => new PointCloudHsv().MakeOutline(),
                    _ => throw new ArgumentException()
                };

                var particlesVerts = newMode switch
                {
                    ColorsDisplayMode.RGB => new PointCloudRgb().ColorsToVertexes(imageColors),
                    ColorsDisplayMode.HSV => new PointCloudHsv().ColorsToVertexes(imageColors),
                    _ => throw new ArgumentException()
                };

                if (particlesVbuf == null)
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
