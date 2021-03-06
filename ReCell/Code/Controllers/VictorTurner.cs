﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Recellection.Code.Models;
using Recellection.Code.Controllers;
using Recellection.Code.Main;
using Recellection.Code.Utility.Logger;
using Recellection.Code.Views;
using Microsoft.Xna.Framework.Audio;

namespace Recellection.Code.Controllers
{
    /// <summary>
    /// 
    /// 
    /// 
    /// Author: John Forsberg
    /// </summary>
    class VictorTurner
    {
        private List<Player> players;

        private World world;

        private GameInitializer gameInitializer;
		private Logger logger = LoggerFactory.GetLogger();

        private WorldController humanControl;
        private GraphController graphControl;
		
        Boolean finished = false;

		private Cue backgroundSound = Sounds.Instance.LoadSound("inGameMusic");
		
        /// <summary>
        /// The constructor used to initiate the Victor Turner
        /// </summary>
        /// <param name="players">The players in the game</param>
        /// <param name="world">The world the game takes place in</param>
        public VictorTurner(GameInitializer gameInitializer)
        {
            this.gameInitializer = gameInitializer;
            this.players = gameInitializer.theWorld.players;
            this.world = gameInitializer.theWorld;
            this.humanControl = new WorldController(players[0],world);
            this.graphControl = GraphController.Instance;
        }

        public void Run()
		{
			backgroundSound.Play();
			
            while (!finished)
            {
                
				logger.Debug("Victor turner is turning the page!");
                foreach (Player player in players)
                {
					if (player is AIPlayer)
					{
						logger.Debug(player.color + " is a AIPlayer!");
						((AIPlayer)player).MakeMove();
					}
					else if (player is Player)
					{
						logger.Debug(player.color+" is human!");
						//This only makes the grid of GUIRegions and scroll zones, remove later.
                        humanControl.Run();
					}
					else
					{
						logger.Fatal("Could not identify "+player.color+" player!");
					}
                    if (CheckIfLostOrWon(players))
                    {
                        finished = true;
                        EndGame(players[0]);
                        break;
                    }
                }
                if (finished)
                {
                    break;
                }
				logger.Info("Weighting graphs!");
                foreach (Player player in players)
                {
                    player.unitAcc.ProduceUnits();
                }

				graphControl.CalculateWeights();

                // This is where we start "animating" all movement
                // FIXME: This ain't okay, hombrey
                // Let the units move!
                logger.Info("Moving units!");
				
				for(int u = 0; u < 5; u++)
				{
					foreach (Player p in players)
					{
						BuildingController.AggressiveBuildingAct(p);
					}
					
					for(int i = 0; i < 100; i++)
					{
						UnitController.Update(world.units, 1, world.GetMap());
						System.Threading.Thread.Sleep(10);
					}
				}

                
            }

        }

		private void EndGame(Player winner)
		{
			if (backgroundSound.IsPlaying)
			{
				backgroundSound.Pause();
			}
			
			humanControl.Stop();
			
			// Build menu
			
			List<MenuIcon> options = new List<MenuIcon>(1);
			MenuIcon cancel = new MenuIcon("");
			cancel.region = new GUIRegion(Recellection.windowHandle, 
				new System.Windows.Rect(0, Globals.VIEWPORT_HEIGHT - 100, Globals.VIEWPORT_WIDTH, 100));
			options.Add(cancel);
			Menu menu = new Menu(options);
			MenuController.LoadMenu(menu);
			
			Recellection.CurrentState = new EndGameView(! (winner is AIPlayer));
			
			MenuController.GetInput();

            MenuController.UnloadMenu();
            
        }

        private void updateFogOfWar(Player player)
        {
            lock (world)
            {
                foreach (Tile t in world.GetMap().map)
                {
                    if (t.GetBuilding() == null)
						continue;
						
                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            if (! world.isWithinMap(x + (int)t.position.X, y + (int)t.position.Y))
								continue;
							
                            //Get the tile and check if it is visible already else set it to visible.
                            if (world.GetMap().GetTile(x + (int)t.position.X, y + (int)t.position.Y).IsVisible(t.GetBuilding().owner))
								continue;

                            // TODO: Do stuff.
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Counts how many graphs a player has, if it is zero
        /// the player has lost. 
        /// 
        /// Condition: when a graph is empty it is
        /// deleted or a empty graph is not counted.
        /// </summary>
        /// <param name="player">The player which might have lost</param>
        /// <returns>True if the player has no graphs false other vice</returns>
        private Boolean HasLost(Player player)
        {
            if (player.GetGraphs().Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This method returns true if there is only one player left playing.
        /// 
        /// </summary>
        /// <returns>Returns true if the length of currently active players 
        /// in the world is zero false other vice.</returns>
        private Boolean HasWon()
        {
            return (world.players.Count == 1);
        }

        private Boolean CheckIfLostOrWon(List<Player> players)
        {
			List<Player> toBeRemoved = new List<Player>();
			foreach(Player p in players)
			{
				if (HasLost(p))
				{
					toBeRemoved.Add(p);
				}
			}
			
			foreach(Player p in toBeRemoved)
			{
                world.players.Remove(p);
			}
			
            if (HasWon())
            {
                return true;
            }
            return false;
        }
    }
}
