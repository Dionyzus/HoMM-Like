using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        GameState currentGameState;
        public GameState CurrentGameState { get => currentGameState; set => currentGameState = value; }
        public static BattleManager BattleManager;
        public static WorldManager WorldManager;

        //Maybe this will need to be an update method
        private void Start()
        {
            currentGameState = LevelManager.instance.gameState;

            if (LevelManager.instance.gameState.Equals(GameState.WORLD))
            {
                instance = WorldManager;
            }
            if (LevelManager.instance.gameState.Equals(GameState.BATTLE))
            {
                instance = BattleManager;
            }
        }
        public enum GameState
        {
            BATTLE,
            WORLD
        }
    }
}
