using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField]
        Texture2D cursor = default;
        [SerializeField]
        Texture2D cursorClicked = default;
        [SerializeField]
        Texture2D cursorOverEnemyUnit = default;
        [SerializeField]
        Texture2D cursorOverCollectable = default;
        [SerializeField]
        Texture2D cursorOverCombatInteraction = default;
        [SerializeField]
        Texture2D interactionInitialized = default;
        [SerializeField]
        Texture2D cursorOverFriendly = default;
        [SerializeField]
        Texture2D rangedAttack = default;

        [SerializeField]
        Texture2D enemyTurnCursor = default;

        private MouseControls mouseControls;

        public static CursorManager instance;

        bool isLeftMouseButtonClicked = false;
        bool isInteractionInitialized = false;
        public MouseControls MouseControls { get => mouseControls; set => mouseControls = value; }

        private void Awake()
        {
            instance = this;
            mouseControls = new MouseControls();

            ChangeCursor(cursor);
            Cursor.lockState = CursorLockMode.Confined;
        }
        private void Start()
        {
            mouseControls.Mouse.Click.started += _ => StartedClick();
            mouseControls.Mouse.Click.performed += _ => EndedClick();
        }

        public void DetectObject(Collider collider)
        {
            InteractionHook hook = collider.GetComponentInParent<InteractionHook>();

            if (hook != null)
            {
                HeroController hero = hook.GetComponentInParent<HeroController>();
                if (hero != null && hero == WorldManager.instance.currentHero)
                    return;

                switch (hook.InteractionType)
                {
                    case InteractionType.FRIENDLY:
                        ChangeCursor(cursorOverFriendly);
                        break;
                    case InteractionType.COLLECTABLE:
                        ChangeCursor(cursorOverCollectable);
                        break;
                    case InteractionType.COMBAT:
                        ChangeCursor(cursorOverCombatInteraction);
                        break;
                    case InteractionType.ENEMY_UNIT:
                        ChangeCursor(cursorOverEnemyUnit);
                        break;
                }
            }
            else
            {
                if (!isLeftMouseButtonClicked && !isInteractionInitialized)
                    ChangeCursor(cursor);
            }
        }
        void StartedClick()
        {
            isLeftMouseButtonClicked = true;
            ChangeCursor(cursorClicked);
        }
        void EndedClick()
        {
            isLeftMouseButtonClicked = false;
            ChangeCursor(cursor);
        }

        private void OnEnable()
        {
            mouseControls.Enable();
        }
        private void OnDisable()
        {
            mouseControls.Disable();
        }
        public void SetToDefault()
        {
            isInteractionInitialized = false;
            ChangeCursor(cursor);
        }
        public void SetToInteractionInitialized()
        {
            isInteractionInitialized = true;
            ChangeCursor(interactionInitialized);
        }
        public void SetToEnemyTurn()
        {
            ChangeCursor(enemyTurnCursor);
        }
        public void SetToRangedAttack()
        {
            ChangeCursor(rangedAttack);
        }
        void ChangeCursor(Texture2D cursorType)
        {
            Cursor.SetCursor(cursorType, Vector2.zero, CursorMode.Auto);
        }
    }
}