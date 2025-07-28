using System;
using System.Linq;
using ChickenFactory.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Espresso;
using Espresso.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace ChickenFactory;

/// <summary>
/// The main class for the game, responsible for managing game components, settings, 
/// and platform-specific configurations.
/// </summary>
public class ChickenFactoryGame : Core
{

    private AnimatedSprite _player;
    
    /// <summary>
    /// Indicates if the game is running on a mobile platform.
    /// </summary>
    public static readonly bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

    /// <summary>
    /// Indicates if the game is running on a desktop platform.
    /// </summary>
    public static readonly bool IsDesktop =
        OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

    /// <summary>
    /// Initializes a new instance of the game. Configures platform-specific settings, 
    /// initializes services like settings and leaderboard managers, and sets up the 
    /// screen manager for screen transitions.
    /// </summary>
    public ChickenFactoryGame() : base("Chicken Factory", 1280, 720, false)
    {
    }

    /// <summary>
    /// Initializes the game, including setting up localization and adding the 
    /// initial screens to the ScreenManager.
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        // Load supported languages and set the default language.
        var cultures = LocalizationManager.GetSupportedCultures();
        var languages = cultures.ToList();

        // TODO You should load this from a settings file or similar,
        // based on what the user or operating system selected.
        const string selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
        LocalizationManager.SetCulture(selectedLanguage);
    }

    /// <summary>
    /// Loads game content, such as textures and particle systems.
    /// </summary>
    protected override void LoadContent()
    {
        //  Create a TextureAtlas instance from the atlas
        var atlas = TextureAtlas.FromFile(Content, "Definitions/Sprites/player_definitions.xml");
        
        // retrieve the slime region from the atlas.
        _player = atlas.CreateAnimatedSprite("player:idle:up");
    }

    /// <summary>
    /// Updates the game's logic, called once per frame.
    /// </summary>
    /// <param name="gameTime">
    /// Provides a snapshot of timing values used for game updates.
    /// </param>
    protected override void Update(GameTime gameTime)
    {
        // Exit the game if the Back button (GamePad) or Escape key (Keyboard) is pressed.
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
            || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _player.Update(gameTime);
        
        base.Update(gameTime);
    }

    /// <summary>
    /// Draws the game's graphics, called once per frame.
    /// </summary>
    /// <param name="gameTime">
    /// Provides a snapshot of timing values used for rendering.
    /// </param>
    protected override void Draw(GameTime gameTime)
    {
        // Clears the screen with the MonoGame orange color before drawing.
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Begin the sprite batch to prepare for rendering.
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the slime texture region at a scale of 4.0
        _player.Draw(SpriteBatch, Vector2.One);

        // Always end the sprite batch when finished.
        SpriteBatch.End();
        
        base.Draw(gameTime);
    }
}