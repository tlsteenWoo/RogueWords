using MknGames._2D;
//using MknGames.Games;
//using MknGames.NonGames;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
//using System;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 
/// </summary>
namespace MknGames
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameMG : Game
    {
        class KeyInfo
        {
            public float timeDown;
        }
        public struct DebugSquareParameters
        {
            public Rectf rectf;
            public Color color;
            public float rotation;
        }
        public bool restrictedInput;
        public bool overrideInputRestriction { get; private set; }
        Dictionary<Keys, KeyInfo> keyInfos;
        Dictionary<int, KeyInfo> mouseInfos;
        public const float mouseHoldThresholdSeconds = 0.5f;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public Texture2D pixel;
        public Texture2D circleTexture;
        public Texture2D diagonalTexture;
        Texture2D toiletGuy;
        Texture2D lockBack, lockFront;
        public Model cubeModel, sphereModel, planeModel;
        public SpriteFont defaultFont, defaultLargerFont;
        public System.Random rand = new System.Random();
        public KeyboardState keyCurrent, keyOld;
        public MouseState mouseCurrent, mouseOld;
        public Vector2 mousePosition;
        public TouchCollection touchesCurrent, touchesOld;
        public Dictionary<int, TouchLocation> touchOrigins = new Dictionary<int, TouchLocation>();
        public BlendState subtractive = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.ReverseSubtract
        };//https://stackoverflow.com/questions/17147628/how-do-i-properly-achieve-subtractive-blending-in-c-xna
        public RasterizerState wireFrameRs = new RasterizerState()
        {
            CullMode = CullMode.None,
            FillMode = FillMode.WireFrame
        };
        public int lineVertexCount;
        public VertexPositionColor[] lineVertices = new VertexPositionColor[100];
        public List<DebugSquareParameters> debugSquares = new List<DebugSquareParameters>(100);
        public int debugSquareCount;
        public BasicEffect lineEffect;
        public Color clearColor = Color.Black;
        public bool enableEscapeKeyToQuit = true;

        //FUNKY VARIABLES (BEGIN)
        public Dictionary<object, object> funkyVariables = new Dictionary<object, object>();
        public bool TryAddFunkyVariable<K, V>(K key, V value)
        {
            if (!funkyVariables.ContainsKey(key))
            {
                funkyVariables.Add(key, value);
                return true;
            }
            return false;
        }
        public V GetFunkyVariable<K, V>(K key, V defaultValue = default(V))
        {
            TryAddFunkyVariable(key, defaultValue);
            return (V)funkyVariables[key];
        }
        public void SetFunkyVariable<K, V>(K key, V value)
        {
            if (!TryAddFunkyVariable(key, value))
            {
                funkyVariables[key] = value;
            }
        }
        //FUNKY VARIABLES (END)

        public GameMG()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            keyInfos = new Dictionary<Keys, KeyInfo>();
            mouseInfos = new Dictionary<int, KeyInfo>();
            mouseInfos.Add(0, new KeyInfo());
            mouseInfos.Add(1, new KeyInfo());
            mouseInfos.Add(2, new KeyInfo());
            var allkeys = Enum.GetValues(typeof(Keys));
            foreach (var val in allkeys)
            {
                keyInfos.Add((Keys)val, new KeyInfo());
            }
            //Components.Add(new RotateScaleAndMatchGame(this));
            //Components.Add(new SteerCollectSurviveGame(this)); //NOTE: Enjoyable this could be expanded upon, but there is not much variety.
            //Components.Add(new RotatePositionScaleAndMatchGame(this));
            //Components.Add(new AddRemoveAndMatchAvoidGame(this));
            //Components.Add(new MultiSquareExperiment(this));
            //Components.Add(new Lighting2DExperiment(this));
            //Components.Add(new EvadeSurviveReproduce(this)); //NOTE: Fun (enjoyable, sorta strategy micro). Pretty limited, cant imagine much else. NOTE(8/21/17): Interesting but less fun, too difficult
            //Components.Add(new MoveBemovedThenDontGame(this)); //NOTE: Ghost Game. Pretty neat
            //Components.Add(new VisualExperiment3D(this));
            //Components.Add(new ScaleAndMatch3DGame(this));
            //Components.Add(new RotateAndMatchGame3D(this));
            //Components.Add(new RotateAndMatch1stPersonGame(this));
            //Components.Add(new ShootPushRepeatGame(this));
            //Components.Add(new RotateShootSurvive3DGame(this)); //NOTE: Depth perception does not seem free in 3D
            //Components.Add(new MoveFlyRotateRaiseLower1stPersonGame(this)); //NOTE: Quite fun doin flips n' s**t
            //Components.Add(new Text2DFusionGame(this));
            //Components.Add(new TranslateGame2D(this)); //Note: neat, Note(8/22/2017): Kinda cool path tracer but fast pace grows tired quick and slow pace isn't really interesting
            //Components.Add(new BondBreakRotateTranslateGame(this));
            //boomerang
            //Components.Add(new BoomerangGame3D(this)); //NOTE: broken, giving up
            //Components.Add(new TranslateZoomRevealGame2D(this)); //NOTE: too much at once, overcomplicated
            //Components.Add(new GhostAndCorpseGame2D(this)); 
            //Components.Add(new Tetris(this));
            //Components.Add(new ExpressionGame(this));

            //Components.Add(new DontFallDontDrop(this));
            //Components.Add(new RogueWordsGame(this));
            //Components.Add(new SmallFPSExpansion(this));
            //Components.Add(new SmallFPS(this));
            //Components.Add(new CapsuleCollisionRoom(this));
            //Components.Add(new SmallFpsCutScene(this));
            //Components.Add(new SmallFPSMenu(this));
            //Components.Add(new BoostRotateRefuelTravel(this)); //Note: Pretty fun, various ways to play
            //Components.Add(new SuperCoolGame(this));
            //Components.Add(new Split_Screen_Dungeon.SplitScreenGame(this));
            //Components.Add(new League_Trainer.LeagueTrainerPro(this));
            //Components.Add(new MknGames._2D.FreerunnerGame(this));
            //Components.Add(new FPSWahtever.FPSWhatever(this));
            //Components.Add(new TheNonExecutive.TheNonExecutiveGame(this));
            //Components.Add(new MakinGamesMenu(this));
            //Components.Add(new CollisionTestRoom(this));
            //Components.Add(new TwoToneMusicSoft(this));
            //Components.Add(new gamegame(this));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            lineEffect = new BasicEffect(GraphicsDevice);
            pixel.SetData<Color>(new Color[] { Color.White });
            //toiletGuy = Content.Load<Texture2D>("toilet");
            //lockBack = Content.Load<Texture2D>("LockBack");
            //lockFront = Content.Load<Texture2D>("LockFront");
            try
            {
                circleTexture = Content.Load<Texture2D>("Sprites/circle-1024");
                diagonalTexture = Content.Load<Texture2D>("Sprites/diagonal");
                cubeModel = Content.Load<Model>("Cube");
                sphereModel = Content.Load<Model>("Sphere");
                planeModel = Content.Load<Model>("Plane");
                //load fonts
                defaultFont = Content.Load<SpriteFont>("default");
                defaultLargerFont = Content.Load<SpriteFont>("default-larger");
            }catch(Exception e)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        public void UnofficialUpdate(double totalMS, double elapsedMS)
        {
            //Update(new GameTime(TimeSpan.FromMilliseconds(totalMS), TimeSpan.FromMilliseconds(elapsedMS)));
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (enableEscapeKeyToQuit && (
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                )
                Exit();

            keyOld = keyCurrent;
            mouseOld = mouseCurrent;
            keyCurrent = Keyboard.GetState();
            mouseCurrent = Mouse.GetState();
            touchesOld = touchesCurrent;
            touchesCurrent = TouchPanel.GetState();
            foreach (TouchLocation touch in touchesCurrent)
            {
                if (tclick(touch))
                {
                    if (!touchOrigins.ContainsKey(touch.Id))
                    {
                        touchOrigins.Add(touch.Id, touch);
                    }
                    else
                    {
                        touchOrigins[touch.Id] = touch;
                    }
                }
            }

            Keys[] pressedKeys = keyCurrent.GetPressedKeys();
            foreach (Keys k in keyInfos.Keys)
            {
                if (kdown(k))
                {
                    keyInfos[k].timeDown += et;
                } else
                {
                    keyInfos[k].timeDown = 0;
                }
            }

            if (mouseCurrent.LeftButton == ButtonState.Pressed)
                mouseInfos[0].timeDown += et;
            else
                mouseInfos[0].timeDown = 0;
            if (mouseCurrent.RightButton == ButtonState.Pressed)
                mouseInfos[1].timeDown += et;
            else
                mouseInfos[1].timeDown = 0;
            if (mouseCurrent.MiddleButton == ButtonState.Pressed)
                mouseInfos[2].timeDown += et;
            else
                mouseInfos[2].timeDown = 0;

            mousePosition = mouseCurrent.Position.ToVector2();

            float w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            float h = GraphicsDevice.PresentationParameters.BackBufferHeight;
            float w2 = Window.ClientBounds.Width;
            float h2 = Window.ClientBounds.Height;
            float scalex = w / w2;
            float scaley = h / h2;
            mousePosition.X *= scalex;
            mousePosition.Y *= scaley;

            base.Update(gameTime);
        }
        public bool InputDenied()
        {
            if (restrictedInput && !overrideInputRestriction)
                return true;
            return false;
        }
        public void BeginUnrestrictedInput()
        {
            if (overrideInputRestriction)
            {
                throw new Exception("End must be called before another begin.");
            } else
            {
                overrideInputRestriction = true;
            }
        }
        public void EndUnrestrictedInput()
        {
            if (!overrideInputRestriction)
            {
                throw new Exception("Begin must be called before another end.");
            }
            else
            {
                overrideInputRestriction = false;
            }
        }
        // helper input, helper mouse
        public bool rmouse
        {
            get
            {
                return mouseCurrent.RightButton == ButtonState.Pressed;
            }
        }
        public bool rmouseOld
        {
            get
            {
                return mouseOld.RightButton == ButtonState.Pressed;
            }
        }
        public bool lmouse
        {
            get
            {
                return mouseCurrent.LeftButton == ButtonState.Pressed;
            }
        }
        public bool lmouseOld
        {
            get
            {
                return mouseOld.LeftButton == ButtonState.Pressed;
            }
        }
        public bool lheld(float timeThresholdSeconds = mouseHoldThresholdSeconds)
        {
            return mouseInfos[0].timeDown > timeThresholdSeconds;
        }
        public bool rheld(float timeThresholdSeconds = mouseHoldThresholdSeconds)
        {
            return mouseInfos[1].timeDown > timeThresholdSeconds;
        }
        public bool mheld(float timeThresholdSeconds = mouseHoldThresholdSeconds)
        {
            return mouseInfos[2].timeDown > timeThresholdSeconds;
        }
        public bool ltap
        {
            get { return lmouse && !lmouseOld; }
        }
        public bool kdown(Keys k)
        {
            if (InputDenied())
                return false;
            return keyCurrent.IsKeyDown(k);
        }
        public bool kclick(Keys k)
        {
            if (InputDenied())
                return false;
            return keyCurrent.IsKeyDown(k) && keyOld.IsKeyUp(k);
        }
        public bool krelease(Keys k)
        {
            if (InputDenied())
                return false;
            return keyCurrent.IsKeyUp(k) && keyOld.IsKeyDown(k);
        }
        public bool kheld(Keys k)
        {
            if (InputDenied())
                return false;
            return keyInfos[k].timeDown > 0.5f;
        }
        public bool kclickheld(Keys key)
        {
            if (InputDenied())
                return false;
            return kclick(key) || kheld(key);
        }
        public bool anyKeyClick()
        {
            if (InputDenied())
                return false;
            Keys[] pressed = keyCurrent.GetPressedKeys();
            foreach (Keys k in pressed)
            {
                if (kclick(k))
                {
                    return true;
                }
            }
            return false;
        }
        public bool touchCollectionContains(TouchCollection collection, TouchLocation touch)
        {
            for (int i = 0; i < collection.Count; ++i)
            {
                if (collection[i].Id == touch.Id)
                    return true;
            }
            return false;
        }
        public bool tclick(TouchLocation touch)
        {
            if (touchCollectionContains(touchesOld, touch))
                return false;
            if (touchCollectionContains(touchesCurrent, touch))
                return true;
            return false;
        }
        public bool trelease(TouchLocation touch)
        {
            if (touch.State == TouchLocationState.Released && touchCollectionContains(touchesCurrent, touch))
            {
                return true;
            }
            return false;
        }
        public TouchLocation torigin(TouchLocation touch)
        {
            return touchOrigins[touch.Id];
        }
        public TouchLocation getTouch(int id)
        {
            for (int t = 0; t < touchesCurrent.Count; ++t)
            {
                if (touchesCurrent[t].Id == id)
                    return touchesCurrent[t];
            }
            throw new ArgumentOutOfRangeException();
        }
        /// <summary>
        /// Returns a number within the min and max based on how far off the ends the givven value is.
        /// </summary>
        /// <param name="value">value to wrap</param>
        /// <param name="min">inclusive minimun</param>
        /// <param name="max">inclusive maximum</param>
        /// <returns></returns>
        public float wrap(float value, float min, float max)
        {
            float range = (max - min) + 1;
            while (value < min) value += range;
            while (value > max) value -= range;
            return value;
        }
        /// <summary>
        /// Creates a random float
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public float randf(float maxValue)
        {
            return (float)rand.NextDouble() * maxValue;
        }
        /// <summary>
        /// Creates a random float
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public float randf(float minValue, float maxValue)
        {
            float range = maxValue - minValue;
            return minValue + (float)rand.NextDouble() * range;
        }

        /// <summary>
        /// Random angle
        /// </summary>
        /// <returns>angle between 0.0 and 2Pi</returns>
        public float randa()
        {
            return randf(0, MathHelper.TwoPi);
        }
        /// <summary>
        /// Wraps angle between 0 and 2*pi
        /// </summary>
        /// <param name="angle">angle to wrap</param>
        /// <returns></returns>
        public float wrapAngle(float angle)
        {
            while (angle < 0)
                angle += MathHelper.TwoPi;
            while (angle > MathHelper.TwoPi)
                angle -= MathHelper.TwoPi;
            return angle;
        }
        public float rotationDist(float rotationA, float rotationB)
        {
            return Math.Abs(rotationDelta(rotationA, rotationB));
        }
        public float rotationDelta(float rotationA, float rotationB)
        {
            float a = rotationA;
            float b = rotationB;
            if (a < 0 || a > MathHelper.TwoPi)
                a = wrapAngle(rotationA);
            if (b < 0 || b > MathHelper.TwoPi)
                b = wrapAngle(rotationB);
            float delta = b - a;
            float dist = Math.Abs(delta);
            float distInverse = MathHelper.TwoPi - dist;
            if (dist < distInverse)
                return delta;
            else
                return distInverse * -Math.Sign(delta);
        }
        public float ScreenWidth
        {
            get { return GraphicsDevice.Viewport.Width; }
        }
        public float ScreenHeight
        {
            get { return GraphicsDevice.Viewport.Height; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>-1 = minimum or less, 1 = maximum or greater, 0 = in between</returns>
        public static float getNumberLineRegion(float val, float min, float max)
        {
            if (val <= min)
                return -1;
            if (val >= max)
                return 1;
            return 0;
        }
        public static float getRelativePosition(float val, float min, float max)
        {
            float range = max - min;
            float center = min + range / 2;
            return val - center;
        }
        public static float getVolumeSphere(float radius)
        {
            //volume of sphere = (4/3) * pi * r^3
            return
                (4.0f / 3.0f) *
                MathHelper.Pi *
                (float)Math.Pow(radius, 3);
        }
        public Vector2 makeDirectional(bool negativeX, bool positiveX, bool negativeY, bool positiveY)
        {
            Vector2 dir = Vector2.Zero;
            if (negativeX)
                dir.X--;
            if (positiveX)
                dir.X++;
            if (negativeY)
                dir.Y--;
            if (positiveY)
                dir.Y++;
            return dir;
        }
        public Vector2 screenCenter()
        {
            return GraphicsDevice.Viewport.Bounds.Center.ToVector2();
        }
        public Vector2 fromAngle(float rotation)
        {
            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }
        public Vector2 randScreenPos()
        {
            return new Vector2(rand.Next(GraphicsDevice.Viewport.Width),
                rand.Next(GraphicsDevice.Viewport.Height));
        }
        public static Vector3 Abs(Vector3 input)
        {
            return new Vector3(Math.Abs(input.X), Math.Abs(input.Y), Math.Abs(input.Z));
        }
        public Color monochrome(float value, float alpha = 1.0f, float hue = 360, float saturation = 0)
        {
            if (saturation > 0)
                return hsl2Rgb(hue, saturation, value, alpha);
            return new Color(value, value, value, alpha);
        }
        public float hue2Rgb(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6 * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
        }
        public Color hsl2Rgb(float h, float s, float l, float alpha = 1)
        {
            float r, g, b;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
                float p = 2 * l - q;
                r = hue2Rgb(p, q, h + 1f / 3f);
                g = hue2Rgb(p, q, h);
                b = hue2Rgb(p, q, h - 1f / 3f);
            }

            return new Color(
                r,
                g,
                b,
                alpha);
        }
        //public BoundingBox makeBox(Vector3 center, Vector3 size)
        //{
        //    return new BoundingBox(center - size / 2, center + size / 2);
        //}
        public static BoundingBox MakeBox(Vector3 center, Vector3 size)
        {
            return new BoundingBox(center - size / 2, center + size / 2);
        }

        void CenterMouse()
        {
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(clearColor);

            base.Draw(gameTime);
        }

        public void drawCircleCentered(Vector2 center, Vector2 size, Color color)
        {
            Vector2 corner = center - size / 2;
            drawCircle(new Rectf(corner.X, corner.Y, size.X, size.Y), color);
        }
        public void drawCircle(Rectf rect, Color color)
        {
            Vector2 textureSize = new Vector2(circleTexture.Width, circleTexture.Height);
            Vector2 position = new Vector2(rect.X, rect.Y);
            Vector2 rectSize = new Vector2(rect.Width, rect.Height);
            Vector2 scale = rectSize / textureSize;
            spriteBatch.Draw(circleTexture,
                position,
                null,
                color,
                0,
                Vector2.Zero,
                scale, SpriteEffects.None, 0);
        }
        public void drawSquare(Vector2 position, Color color, float radians, float width, float height, float depth = 0)
        {
            drawTexture(pixel, position, color, radians, width, height, depth, new Vector2(0.5f, 0.5f));
        }
        public void drawSquare(Rectangle rect, Color color, float radians)
        {
            spriteBatch.Draw(pixel, rect, null, color,
                radians,
                Vector2.Zero,
                SpriteEffects.None, 0);
        }
        public void drawSquare(Rectf rect, Color color, float radians)
        {
            drawSquare(new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2), color, radians, rect.Width, rect.Height);
        }
        public void drawTexture(Texture2D texture, Vector2 position, Color color, float radians, float width, float height, float depth = 0, Vector2 origin = new Vector2())
        {
            spriteBatch.Draw(texture, position, null, color,
                radians,
                origin, new Vector2(width, height) / new Vector2(texture.Width, texture.Height), SpriteEffects.None, depth);
        }
        public void drawLine(Vector2 pointA, Vector2 pointB, Color color, float thickness, float depth = 0)
        {
            Vector2 line = pointB - pointA;
            float rotation = (float)Math.Atan2(line.Y, line.X);
            drawSquare(pointA + line / 2, color, rotation, line.Length(), thickness, depth);
        }
        public void drawLine(Vector2 origin, float angle, float length, Color color, float thickness, float depth = 0)
        {
            Vector2 dir = fromAngle(angle);
            Vector2 line = dir * length;
            drawSquare(origin + line / 2, color, angle, line.Length(), thickness, depth);
        }
        public void drawFrame(Vector2 position, Color color, float width, float height, float thickness, float depth, float rotation)
        {
            Rectangle rect = centeredRect(position, width, height);
            Vector2 left = new Vector2(position.X - width / 2 + thickness / 2, position.Y);
            Vector2 right = new Vector2(position.X + width / 2 - thickness / 2, position.Y);
            Vector2 top = new Vector2(position.X, position.Y - height / 2 + thickness / 2);
            Vector2 bot = new Vector2(position.X, position.Y + height / 2 - thickness / 2);
            Matrix matRotZ = Matrix.CreateTranslation(-position.X, -position.Y, 0) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0);
            left = Vector2.Transform(left, matRotZ);
            right = Vector2.Transform(right, matRotZ);
            top = Vector2.Transform(top, matRotZ);
            bot = Vector2.Transform(bot, matRotZ);
            drawSquare(left, color, rotation, thickness, height, depth);
            drawSquare(right, color, rotation, thickness, height, depth);
            drawSquare(top, color, rotation, width, thickness, depth);
            drawSquare(bot, color, rotation, width, thickness, depth);
        }
        public void drawFrame(Rectangle rect, Color color, float thickness, float depth = 0, bool growInward = true)
        {
            drawFrame(rect.Center.ToVector2(), color, rect.Width, rect.Height, thickness, depth, growInward);
        }
        public void drawFrame(Vector2 position, Color color, float width, float height, float thickness, float depth = 0, bool growInward = true)
        {
            Rectangle rect = centeredRect(position, width, height);
            float thicknessDirection = 1;
            if (growInward)
                thicknessDirection = -1;
            float shift = thickness / 2 * thicknessDirection;
            Vector2 left =
                new Vector2(
                position.X - width / 2 - shift,
                position.Y);
            Vector2 right =
                new Vector2(
                position.X + width / 2 + shift,
                position.Y);
            Vector2 top =
                new Vector2(
                    position.X,
                    position.Y - height / 2 - shift);
            Vector2 bot =
                new Vector2(
                    position.X,
                    position.Y + height / 2 + shift);
            float boxHeight = height;
            float frameWidth = width;
            if (!growInward)
            {
                //boxHeight += thickness;
                frameWidth += thickness;
            }
            drawSquare(left, color, 0, thickness, boxHeight, depth);
            drawSquare(right, color, 0, thickness, boxHeight, depth);
            drawSquare(top, color, 0, frameWidth, thickness, depth);
            drawSquare(bot, color, 0, frameWidth, thickness, depth);
        }
        public void drawStringf(SpriteFont font, string text, Rectf rect, Color color, Vector2 normalizedOrigin = default(Vector2), bool fitToContainer = false, float uniformScale = 1)
        {
            drawString(defaultLargerFont, text, new Vector2(rect.Width, rect.Height), new Vector2(rect.X, rect.Y), color, normalizedOrigin, fitToContainer, uniformScale);
        }
        public void drawString(string text, Rectangle container, Color color, Vector2 normalizedOrigin = default(Vector2), bool fitToContainer = false)
        {
            drawString(text, container.Size.ToVector2(), container.Location.ToVector2(), color, normalizedOrigin, fitToContainer);
        }
        public void drawString(string text, Vector2 containerSize, Vector2 containerPosition, Color color, Vector2 normalizedOrigin = default(Vector2), bool fitToContainer = false)
        {
            drawString(defaultFont, text, containerSize, containerPosition, color, normalizedOrigin, fitToContainer);
        }
        public void drawString(SpriteFont font, string text, Rectangle rect, Color color, Vector2 normalizedOrigin = default(Vector2), bool fitToContainer = false, float uniformScale = 1)
        {
            drawString(defaultLargerFont, text, rect.Size.ToVector2(), rect.Location.ToVector2(), color, normalizedOrigin, fitToContainer, uniformScale);
        }
        public void drawString(SpriteFont font, string text, Vector2 containerSize, Vector2 containerPosition, Color color, Vector2 normalizedOrigin = default(Vector2), bool fitToContainer = false, float uniformScale = 1)
        {
            Vector2 size = font.MeasureString(text); //default text size
            Vector2 scale = Vector2.One;// new Vector2(minScale, minScale); //scale to apply to text
            if (fitToContainer)
            {
                Vector2 squish = containerSize / size; //ratio of container size to default text size
                float minScale = Math.Min(squish.X, squish.Y); //smallest scale needed to go from default text size to container size
                scale = new Vector2(minScale, minScale); //scale to apply to text
            }
            scale *= uniformScale;
            Vector2 finalSize = size * scale; //size of text after scaling
            Vector2 difference = containerSize - finalSize; //space remaining in container
            Vector2 position = containerPosition + normalizedOrigin * difference;
            spriteBatch.DrawString(font, text, position, color, 0, Vector2.Zero, scale,
                SpriteEffects.None, 0);
        }
        public Rectf CalculateTextContainer(SpriteFont font, string text, Rectf rect, Color color, Vector2 normalizedOrigin = default(Vector2), bool fitToContainer = false, float uniformScale = 1)
        {
            return CalculateTextContainer(defaultLargerFont, text, new Vector2(rect.Width, rect.Height), new Vector2(rect.X, rect.Y), color, normalizedOrigin, fitToContainer, uniformScale);
        }
        public Rectf CalculateTextContainer(SpriteFont font, string text, Rectangle rect, Color color, Vector2 normalizedOrigin = default(Vector2), bool fitToContainer = false, float uniformScale = 1)
        {
            return CalculateTextContainer(defaultLargerFont, text, rect.Size.ToVector2(), rect.Location.ToVector2(), color, normalizedOrigin, fitToContainer, uniformScale);
        }
        public Rectf CalculateTextContainer(SpriteFont font, string text, Vector2 containerSize, Vector2 containerPosition, Color color, Vector2 normalizedOrigin = default(Vector2), bool fitToContainer = false, float uniformScale = 1)
        {
            Vector2 size = font.MeasureString(text); //default text size
            Vector2 scale = Vector2.One;// new Vector2(minScale, minScale); //scale to apply to text
            if (fitToContainer)
            {
                Vector2 squish = containerSize / size; //ratio of container size to default text size
                float minScale = Math.Min(squish.X, squish.Y); //smallest scale needed to go from default text size to container size
                scale = new Vector2(minScale, minScale); //scale to apply to text
            }
            scale *= uniformScale;
            Vector2 finalSize = size * scale; //size of text after scaling
            Vector2 difference = containerSize - finalSize; //space remaining in container
            Vector2 position = containerPosition + normalizedOrigin * difference;
            return new Rectf(position.X, position.Y, finalSize.X, finalSize.Y);
        }
        public Vector2[] makeNgon(Vector2 position, float rotation, int sides, float radius)
        {
            Vector2[] vertices = new Vector2[sides];
            float step = MathHelper.TwoPi / (float)sides;
            Vector2 epicenter = Vector2.Zero;
            for (int i = 0; i < sides; ++i)
            {
                float rot = rotation + step * (float)(i);
                Vector2 vertex = position + fromAngle(rot) * radius;
                epicenter += vertex;
                vertices[i] = vertex;
            }
            return vertices;
        }
        public void drawNgon(Vector2 position, Color color, float rotation, int sides, float radius, float thickness, float depth = 0)
        {
            drawNgon(makeNgon(position, rotation, sides, radius), color, thickness, depth);
        }
        public void drawNgon(Vector2[] vertices, Color color, float thickness, float depth = 0)
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                int a = i;
                int b = i + 1;
                if (i == vertices.Length - 1)
                    b = 0;
                drawLine(vertices[a], vertices[b], color, thickness, depth);
            }
        }
        public Rectangle centeredRect(Vector2 position, float width, float height)
        {
            Rectangle rect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)width,
                (int)height);
            rect.X -= rect.Width / 2;
            rect.Y -= rect.Height / 2;
            return rect;
        }
        public void DrawModel(Model model, Matrix world, Matrix view, Matrix projection, Color color, bool lightingEnabled = true, Texture2D texture = null)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    BasicEffect basicEffect = part.Effect as BasicEffect;
                    if (basicEffect != null)
                    {
                        if (lightingEnabled)
                        {
                            basicEffect.EnableDefaultLighting();
                        } else
                        {
                            basicEffect.LightingEnabled = false;
                        }
                        basicEffect.DiffuseColor = color.ToVector3();
                        basicEffect.Alpha = (float)color.A / 255.5f;
                        basicEffect.PreferPerPixelLighting = false;
                        basicEffect.View = view;
                        basicEffect.Projection = projection;
                        basicEffect.World = world;
                        basicEffect.Texture = texture;
                        basicEffect.TextureEnabled = texture != null;
                    }
                }
                mesh.Draw();
            }
        }

        public void add3DLine(Vector3 A, Vector3 B, Color color)
        {
            add3DLine(A, B, color, color);
        }
        public void add3DLine(Vector3 A, Vector3 B, Color colorA, Color colorB)
        {
            addVertex(A, colorA);
            addVertex(B, colorB);
        }
        public void addVertex(Vector3 pos, Color color)
        {
            if (lineVertices.Length < ++lineVertexCount)
            {
                VertexPositionColor[] larger = new VertexPositionColor[lineVertices.Length * 3];
                for (int i = 0; i < lineVertices.Length; ++i)
                {
                    larger[i] = lineVertices[i];
                }
                lineVertices = larger;
            }
            if (lineVertices[lineVertexCount - 1] == null)
            {
                lineVertices[lineVertexCount - 1] = new VertexPositionColor();
            }
            lineVertices[lineVertexCount - 1].Position = pos;
            lineVertices[lineVertexCount - 1].Color = color;
        }
        public void debugSquare(Rectf _rect, Color _color, float _rotation)
        {
            if (debugSquares.Count <= debugSquareCount)
                debugSquares.Add(new DebugSquareParameters());
            debugSquares[debugSquareCount++] = new DebugSquareParameters() { rectf = _rect, color = _color, rotation = _rotation };
        }
        public void DrawDebugSquares()
        {
            for (int i = 0; i < debugSquareCount; ++i)
            {
                drawSquare(debugSquares[i].rectf, debugSquares[i].color, debugSquares[i].rotation);
            }
        }
        public void FlushDebugSquares()
        {
            debugSquareCount = 0;
        }
        /// <summary>
        /// Be sure to flush after drawing or lines will accumulate!
        /// </summary>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        public void DrawAll3dLines(Matrix view, Matrix projection)
        {
            if (lineVertexCount > 0)
            {
                lineEffect.LightingEnabled = false;
                lineEffect.World = Matrix.Identity;
                lineEffect.View = view;
                lineEffect.Projection = projection;
                lineEffect.VertexColorEnabled = true;
                lineEffect.DiffuseColor = Vector3.One;
                lineEffect.Alpha = 1;
                lineEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList,
                    lineVertices, 0, lineVertexCount / 2, VertexPositionColor.VertexDeclaration);
            }
        }
        public void Flush3dLines()
        {
            lineVertexCount = 0;
        }
    }
}
