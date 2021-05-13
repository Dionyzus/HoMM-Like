using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class LevelManager : MonoBehaviour
    {
        public GameState gameState;

        public static LevelManager instance;

        private void Awake()
        {
            instance = this;
        }

        public void SetGameState(GameState gameState)
        {
            GameManager.instance.CurrentGameState = gameState;
        }
    }
}