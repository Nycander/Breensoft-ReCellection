﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Recellection.Code.Utility.Events;
using Recellection.Code.Utility.Logger;
using Microsoft.Xna.Framework;

namespace Recellection.Code.Models
{
    /// <summary>
    /// Part of the model describing the game world. Contains a list of the players and the matrix
    /// of tiles that make up the game map.
    /// </summary>
    public class World : IModel
    {
        public Logger myLogger;

        private static int maxCols = (int)((float)Recellection.viewPort.Width / (float)Globals.TILE_SIZE);
        private static int maxRows = (int)((float)Recellection.viewPort.Height / (float)Globals.TILE_SIZE);

        private Point lookingAt;
        public Point LookingAt
        {
            get
            {
                return lookingAt;
            }
            set
            {
                lookingAt = value;
                lookingAt.X = (int)MathHelper.Clamp(lookingAt.X, 0, map.width - maxCols);
                lookingAt.Y = (int)MathHelper.Clamp(lookingAt.Y, 0, map.height - maxRows);
                if (lookingAtEvent != null)
                {
                    lookingAtEvent(this, new Event<Point>(value, EventType.ALTER));
                }
            }
        }

        public event Publish<Point> lookingAtEvent;

        # region Inner Class Map
        /// <summary>
        /// This class is a wrapper for the Tile matrix used by the world.
        /// It has functions for setting and getting tiles on certain locations.
        /// </summary>
        public class Map : IModel
        {
            public event Publish<Tile> TileEvent;

            public Tile[, ] map { get; private set; }

            public int height { get; private set; }

            public int width { get; private set; }

            /// <summary>
            /// Constructs a new Map model from a matrix of tiles.
            /// </summary>
            /// <param name="map">The matrix of tiles that will form the map</param>
            public Map(Tile[,] map)
            {
                this.map = map;
                this.width = map.GetLength(0);
                this.height = map.GetLength(1);
            }


            /// <summary>
            /// Retrieve the tile at row row and column col in the map. Invokes an EMapEvent.
            /// </summary>
            /// <param name="row">The row of the tile to be retrieved</param>
            /// <param name="col">The column of the tile to be retrieved</param>
            /// <returns></returns>
            public Tile GetTile(int x, int y)
            {
                if (x < 0 || y < 0 || x > width || y > height)
                {
                    throw new IndexOutOfRangeException("Attempted to set a tile outside the range of the map.");
                }
                return map[x, y];
            }
            
            public Tile GetTile(Point p)
            {
				return GetTile(p.X, p.Y);
			}

			public Tile GetTile(Vector2 p)
			{
				return GetTile((int)p.X, (int)p.Y);
			}

            /// <summary>
            /// Set a tile in the world. Invokes the MapEvent event. Will throw 
            /// IndexOutOfRangeException if placement of a tile was attempted outside
            /// the range of the map.
            /// </summary>
            /// <param name="row">The row in which the tile will be set (index 0).</param>
            /// <param name="col">The column in which the tile will be set (index 0).</param>
            /// <param name="t">Tile tile to be set.</param>
            public void SetTile(int x, int y, Tile t)
            {
                if (x < width || y < height)
                {
                    throw new IndexOutOfRangeException("Attempted to set a tile outside the range of the map.");
                }
                map[x, y] = t;
                if (TileEvent != null)
                {
                    TileEvent(this, new Event<Tile>(t, EventType.REMOVE));
                }
            }
        }

        # endregion


        #region Events
        
        /// <summary>
        /// Event that is invoked when the map is changed
        /// </summary>
        public event Publish<Map> MapEvent;

        /// <summary>
        /// Event that is invoked when the set of players in the world changes
        /// </summary>
        public event Publish<Player> PlayerEvent; 
        #endregion
        
        /// <summary>
        /// The tiles of the world arranged in a row-column matrix
        /// </summary>
        public Map map { get; private set; }

        public int seed { get; private set; }

        public List<Player> players { get; private set; }

        public HashSet<Unit> units { get; private set; }

        public List<Point> DrawConstructionLines { get; set; }

        /// <summary>
        /// Constructor of the game world. Creates an empty list of players
        /// as well as an empty matrix of the given dimensions.
        /// </summary>
        /// <param name="rows">Number of rows in the map of the world</param>
        /// <param name="cols">Number of columns in the map of the world</param>
        public World(Tile[,] map, int seed)
        {
            myLogger = LoggerFactory.GetLogger();
            players = new List<Player>();
            this.seed = seed;
            SetMap(map);
			this.lookingAt = Point.Zero;
            this.units = new HashSet<Unit>();
        }

        /// <summary>
        /// Add a player to the game world. Invokes the PlayerEvent event.
        /// </summary>
        /// <param name="p">The player to be added to the world</param>
        public void AddPlayer(Player p) 
        {
            players.Add(p);
            if (PlayerEvent != null)
            {
                PlayerEvent(this, new Event<Player>(p, EventType.ADD));
            }
        }

        /// <summary>
        /// Remove a player from the game world. Invokes the PlayerEvent event.
        /// </summary>
        /// <param name="p">The player to be removed</param>
        public void RemovePlayer(Player p)
        {
            players.Remove(p);
            if (PlayerEvent != null)
            {
                PlayerEvent(this, new Event<Player>(p, EventType.REMOVE));
            }
        }

        /// <summary>
        /// Sets the game map to a specific Tile-matrix. Dimensions are required.
        /// Invokes two EMapEvents.
        /// </summary>
        /// <param name="map">The Tilematrix to be the new map</param>
        /// <param name="rows">The number of rows in the new map</param>
        /// <param name="cols">The number of columns in the new map</param>
        public void SetMap(Tile[,] map)
        {
            ClearMap();
            this.map = new Map(map);
            if (MapEvent!=null)
            {
                MapEvent(this, new Event<Map>(this.map, EventType.ADD)); 
            }
        }

        /// <summary>
        /// Empty the game map. Invokes an EMapEvent.
        /// </summary>
        public void ClearMap()
        {
            this.map = null;
            Map temp = this.map;
            if (MapEvent != null)
            {
                MapEvent(this, new Event<Map>(temp, EventType.REMOVE));
            }
        }

        /// <summary>
        /// Returns the matrix of Tiles that is the game map
        /// </summary>
        /// <returns></returns>
        public World.Map GetMap()
        {
            return map;
        }

        public bool isWithinMap(int x, int y)
        {
            return x > 0 && x < map.width && y > 0 && y < map.height;
        }


        public void AddUnit(Unit u)
        {
            lock (units)
            {
                units.Add(u);
            }
        }

        public void RemoveUnit(Unit u)
        {
            lock (units)
            {
                units.Remove(u);
            }
        }
    }
}