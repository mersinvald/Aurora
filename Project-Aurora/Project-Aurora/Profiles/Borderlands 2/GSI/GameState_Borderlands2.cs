﻿using Aurora.Profiles.Borderlands2.GSI.Nodes;
using System;

namespace Aurora.Profiles.Borderlands2.GSI
{
    /// <summary>
    /// A class representing various information relating to Borderlands 2
    /// </summary>
    public class GameState_Borderlands2 : GameState<GameState_Borderlands2>
    {
        private Player_Borderlands2 player;

        /// <summary>
        /// Information about the local player
        /// </summary>
        public Player_Borderlands2 Player
        {
            get
            {
                if (player == null)
                    player = new Player_Borderlands2("");

                return player;
            }
        }

        /// <summary>
        /// Creates a default GameState_Borderlands2 instance.
        /// </summary>
        public GameState_Borderlands2() : base()
        {
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_Borderlands2(string json_data) : base(json_data)
        {
        }

        /// <summary>
        /// A copy constructor, creates a GameState_Borderlands2 instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState_Borderlands2(IGameState other_state) : base(other_state)
        {
        }
    }
}
