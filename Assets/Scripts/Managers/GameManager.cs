using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace HOMM_BM
{
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        [HideInInspector]
        public Keyboard Keyboard;
        [HideInInspector]
        public Mouse Mouse;

        public bool IsMouseOverGameWindow
        {
            get
            {
                Vector2 mousePosition = Mouse.position.ReadValue();

                return !(0 > mousePosition.x || 0 > mousePosition.y
                    || Screen.width < mousePosition.x
                    || Screen.height < mousePosition.y);
            }
        }

        [SerializeField]
        GameState currentGameState;
        public GameState CurrentGameState { get => currentGameState; set => currentGameState = value; }
        public bool StateInitialized { get => stateInitialized; set => stateInitialized = value; }

        private void Awake()
        {
            instance = this;
        }

        //Maybe this will need to be an update method
        private void Start()
        {
            Keyboard = InputSystem.GetDevice<Keyboard>();
            Mouse = InputSystem.GetDevice<Mouse>();
        }

        bool stateInitialized;

        private void Update()
        {
            if (LevelManager.instance != null)
            {
                currentGameState = LevelManager.instance.gameState;

                if (!stateInitialized)
                {
                    if (LevelManager.instance.gameState.Equals(GameState.WORLD))
                    {
                        WorldManager.instance.Initialize();
                        stateInitialized = true;
                    }
                    if (LevelManager.instance.gameState.Equals(GameState.BATTLE))
                    {
                        BattleManager.instance.Initialize();
                        stateInitialized = true;
                    }
                }
            }
        }
    }
}