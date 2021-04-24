using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        GameState currentGameState;
        public GameState CurrentGameState { get => currentGameState; set => currentGameState = value; }

        private void Awake()
        {
            instance = this;
        }

        //Maybe this will need to be an update method
        private void Start()
        {
            currentGameState = LevelManager.instance.gameState;

            if (LevelManager.instance.gameState.Equals(GameState.WORLD))
            {
                WorldManager.instance.Initialize();
            }
            if (LevelManager.instance.gameState.Equals(GameState.BATTLE))
            {
                BattleManager.instance.Initialize();
            }
        }

        public enum GameState
        {
            BATTLE,
            WORLD
        }
    }
}