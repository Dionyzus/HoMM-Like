using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class LevelManager : MonoBehaviour
    {
        public Enums.GameState gameState;

        public static LevelManager instance;

        private void Awake()
        {
            instance = this;
        }

        public void SetGameState(Enums.GameState gameState)
        {
            GameManager.instance.CurrentGameState = gameState;
        }
    }
}