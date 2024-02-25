using HexChess.Core;
using HexChess.Screens.ChessScreen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;


namespace HexChess
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;

        private readonly ScreenManager _screenManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
        }

        internal void LoadChessScreen(InitialisationOptions options)
        {
            _screenManager.LoadScreen(new Screens.ChessScreen.ChessScreen(this, options), new FadeTransition(_graphics.GraphicsDevice, Color.Black));
        }

        internal void LoadStartScreen()
        {
            _screenManager.LoadScreen(new Screens.StartScreen.StartScreen(this), new FadeTransition(_graphics.GraphicsDevice, Color.Black));
        }

        internal void LoadPieceCellTableScreen()
        {
            _screenManager.LoadScreen(new Screens.PieceCellTableScreen.PieceCellTableScreen(this), new FadeTransition(_graphics.GraphicsDevice, Color.Black));
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Myra.MyraEnvironment.Game = this;

            Initialisation.Initialise();

            LoadStartScreen();
            //LoadChessScreen(new InitialisationOptions() { BlackPlayer = InitialisationOptions.PlayerType.Ai });
            //LoadPieceCellTableScreen();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            SpriteLibrary.LoadSprites(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            Window.Title = Mouse.GetState().Position.ToString();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}