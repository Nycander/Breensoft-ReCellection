﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Recellection.Code.Models;
using Recellection.Code.Controllers;

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

        Boolean finished = false;
        /// <summary>
        /// The constructor used to initiate the Victor Turner
        /// </summary>
        /// <param name="players">The players in the game</param>
        /// <param name="world">The world the game takes place in</param>
        public VictorTurner(List<Player> players,World world)
        {
            this.players = players;
            this.world = world;
        }

        public void Run()
        {

            while (!finished)
            {
                foreach (Player player in players)
                {
                    if(HasLost(player))
                    {
                        world.RemovePlayer(player);
                    }
                    if(HasWon())
                    {

                        finished = true;
                        break;  
                    }
                    new WorldController(player);
                }
                //UnitController.Update(world);

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
            if (world.players.Count == 1)
            {
                return true;
            }

            return false;
        }
    }
}