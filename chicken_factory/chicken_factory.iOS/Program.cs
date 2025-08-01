﻿using ChickenFactory;
using Foundation;
using UIKit;

namespace chicken_factory.iOS
{
    [Register("AppDelegate")]
    internal class Program : UIApplicationDelegate
    {
        private static ChickenFactoryGame _game;

        /// <summary>
        /// Initializes and starts the game by creating an instance of the 
        /// Game class and calls its Run method.
        /// </summary>
        internal static void RunGame()
        {
            _game = new ChickenFactoryGame();
            _game.Run();
        }

        /// <summary>
        /// Called when the application has finished launching. 
        /// This method starts the game by calling RunGame.
        /// </summary>
        /// <param name="app">The UIApplication instance representing the application.</param>
        public override void FinishedLaunching(UIApplication app)
        {
            RunGame();
        }

        /// <summary>
        /// The main entry point for the application. 
        /// This sets up the application and specifies the UIApplicationDelegate 
        /// class to handle application lifecycle events.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, typeof(Program));
        }
    }
}