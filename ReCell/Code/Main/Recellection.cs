using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Recellection.Code.Utility.Console;
using Recellection.Code.Utility.Logger;
using Recellection.Code.Views;
using Recellection.Code.Models;
using System.Threading;
using Recellection.Code.Controllers;

/*
 * BREENSOFT GAME OMG OMG OMG
 * 
 * Authors:
 */


namespace Recellection
{
    /// <summary>
    /// This is the main type for your game.
	/// It's the SEX-na thread! / Joel
    /// </summary>
    public class Recellection : Microsoft.Xna.Framework.Game
    {
        private static Logger logger = LoggerFactory.GetLogger("XNA");

        public static SpriteTextureMap textureMap;
        public static IntPtr windowHandle;
        public static Viewport viewPort;
        public static SpriteFont screenFont;
        public static Color breen = new Color(new Vector3(0.4f, 0.3f, 0.1f));
        public static GraphicsDeviceManager graphics;
        public static ContentManager contentMngr;
        public Thread LogicThread { get; set; }

        //FPS
        int frameRate;
        int frameCounter;
        TimeSpan elapsedTime;
        
        TobiiController tobiiController;
        public static KeyboardState publicKeyBoardState;
        SpriteBatch spriteBatch;

		public static SpriteFont worldFont;

        // Current state!
        private static IView currentState;
        public static IView CurrentState
        { 
			get { return currentState; }
			set { currentState = value; }
		}
        

        // Python console
        SpriteFont consoleFont;
        PythonInterpreter console;

        //Sounds and music
        AudioPlayer audioPlayer;

        //Debug Input 
        KeyboardState lastKBState, kBState;
        MouseState lastMouseState, mouseState;
       

        public Recellection()
        {
            tobiiController = TobiiController.GetInstance(this.Window.Handle);
            tobiiController.Init();
            graphics = new GraphicsDeviceManager(this);            
			Content.RootDirectory = "Content";

            contentMngr = Content;
        }

        /// <summary>
        /// Initialize all classes and set renderstates
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

#if DEBUG
            // Initialize the python console
            console = new PythonInterpreter(this, consoleFont);
			console.AddGlobal("game", this);
#endif

			windowHandle = this.Window.Handle;
            
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(10000000L / 30L);
            
            //graphics.ApplyChanges();
            LogicThread.Start();
        }
        
        public void lawl(string sound)
		{
			audioPlayer.PlaySound(sound);
        }

        /// <summary>
        /// Create models and load game data
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            screenFont = Content.Load<SpriteFont>("Fonts/ScreenFont");
            consoleFont = Content.Load<SpriteFont>("Fonts/ConsoleFont");
			worldFont = Content.Load<SpriteFont>("Fonts/WorldFont");

            audioPlayer = new AudioPlayer(Content);
            audioPlayer.PlaySong(Globals.Songs.Theme);

            viewPort = graphics.GraphicsDevice.Viewport;
        }

        /// <summary>
        /// Not Used
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
		{
			if (currentState != null)
			{
				currentState.Update(gameTime);
			}
            HandleDebugInput();

            #region FPS Stuff
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
            #endregion

            base.Update(gameTime);
        }

        /// <summary>
        /// This method allows us to use the keyboard and mouse to test functionality in the program
        /// </summary>
        private void HandleDebugInput()
        {
#if DEBUG
            // If the console is open, we ignore input.
            if (console.IsActive())
            {
                return;
            }
#endif
            #region Update input states

            lastKBState = kBState;
            lastMouseState = mouseState;
            publicKeyBoardState = Keyboard.GetState();
            kBState = publicKeyBoardState;
            mouseState = Mouse.GetState();

            #endregion

			if (kBState.IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}
			
			if (kBState.IsKeyDown(Keys.End))
			{
				WorldController.finished = true;
			}
			
            if (kBState.IsKeyDown(Keys.F) && lastKBState.IsKeyUp(Keys.F))
            {
                System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
                if (form.Width > 800)
                {

                    form.Location = new System.Drawing.Point(0, 0);
                    form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    form.TopMost = true;
                    Recellection.graphics.PreferredBackBufferWidth = 800;
                    Recellection.graphics.PreferredBackBufferHeight = 600;
                    Recellection.graphics.ApplyChanges();
                }
                else
                {                    
                    form.Location = new System.Drawing.Point(0, 0);
                    form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    form.TopMost = true;
                    Recellection.graphics.PreferredBackBufferWidth = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
                    Recellection.graphics.PreferredBackBufferHeight = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
                    Recellection.graphics.ApplyChanges();
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (currentState is WorldView)
                WorldView.Instance.RenderToTex(spriteBatch, gameTime);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            if (currentState != null)
            {
                currentState.Draw(spriteBatch);
            }
#if DEBUG
            spriteBatch.DrawString(screenFont, frameRate.ToString(), Vector2.Zero, Color.Red);
#endif
            spriteBatch.End();
            // Do the FPS dance
            frameCounter++;

            base.Draw(gameTime);
        }

		public void ToggleLogger(String s)
		{
			if (LoggerFactory.HasLogger(s))
			{
				LoggerFactory.GetLogger(s).Active = !LoggerFactory.GetLogger(s).Active;
			}
			else
			{
				console.Console.WriteLine("Logger does not exist");
			}

		}

		public void ListLoggers()
		{
			console.Console.WriteLine(LoggerFactory.ListLoggers());
		}

		public void SetLoggers(int i)
		{
			bool active = false;
			if (i != 0)
				active = false;
			LoggerFactory.SetAll(active);
		}

		public void LogAI()
		{
			ToggleLogger("Recellection.Code.Controllers.AIPlayer");
			ToggleLogger("Recellection.Code.AIView");
		}

        public void Help()
        {
            console.Console.WriteLine("M: Toggle music\nI: Turn SFX off\nO: Turn SFX on\nA: Acid sound\nB: Explosion sound\nF1: Toggle Console\nF: \"full\" screen");
        }
		
		public void tudeloo()
		{
			playBeethoven();
		}

		public static void playBeethoven()
		{
			Console.Beep(659, 120);  // Treble E
			Console.Beep(622, 120);  // Treble D#

			Thread.Sleep(60);

			Console.Beep(659, 120);  // Treble E
			Console.Beep(622, 120);  // Treble D#
			Console.Beep(659, 120);  // Treble E
			Console.Beep(494, 120);  // Treble B
			Console.Beep(587, 120);  // Treble D
			Console.Beep(523, 120);  // Treble C

			Thread.Sleep(70);

			Console.Beep(440, 120);  // Treble A
			Console.Beep(262, 120);  // Middle C
			Console.Beep(330, 120);  // Treble E
			Console.Beep(440, 120);  // Treble A

			Thread.Sleep(70);

			Console.Beep(494, 120);  // Treble B
			Console.Beep(330, 120);  // Treble E
			Console.Beep(415, 120);  // Treble G#
			Console.Beep(494, 120);  // Treble B

			Thread.Sleep(70);

			Console.Beep(523, 120);  // Treble C
			Console.Beep(330, 120);  // Treble E
			Console.Beep(659, 120);  // Treble E
			Console.Beep(622, 120);  // Treble D#

			Thread.Sleep(70);

			Console.Beep(659, 120);  // Treble E
			Console.Beep(622, 120);  // Treble D#
			Console.Beep(659, 120);  // Treble E
			Console.Beep(494, 120);  // Treble B
			Console.Beep(587, 120);  // Treble D
			Console.Beep(523, 120);  // Treble C

			Thread.Sleep(70);

			Console.Beep(440, 120);  // Treble A
			Console.Beep(262, 120);  // Middle C
			Console.Beep(330, 120);  // Treble E
			Console.Beep(440, 120);  // Treble A

			Thread.Sleep(70);

			Console.Beep(494, 120);  // Treble B
			Console.Beep(330, 120);  // Treble E
			Console.Beep(523, 120);  // Treble C
			Console.Beep(494, 120);  // Treble B
			Console.Beep(440, 120);  // Treble A
		}
    }
}
