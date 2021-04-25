using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HOMM_BM
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager instance;

        public HeroController currentHero;
        [HideInInspector]
        public bool heroIsMoving;

        MouseLogicWorld currentMouseLogic;
        public MouseLogicWorld selectMove;

        private void Awake()
        {
            instance = this;
        }
        public void Initialize()
        {
            HeroController[] heroes = FindObjectsOfType<HeroController>();
            foreach (HeroController hero in heroes)
            {
                if (hero.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                {
                    UiManager.instance.AddHeroButton(hero);
                    UiManager.instance.AddStepsSlider(hero);
                }
            }
        }

        private void Update()
        {
            if (!GameManager.instance.CurrentGameState.Equals(GameManager.GameState.WORLD))
                return;

            if (currentHero != null)
            {
                if (currentHero.IsInteracting || currentHero.IsInteractionInitialized)
                    return;

                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit, 100))
                    {
                        if (EventSystem.current.IsPointerOverGameObject())
                        {
                            InteractionHook hook = hit.transform.GetComponentInParent<InteractionHook>();

                            if (hook != null)
                            {
                                currentHero.currentInteractionHook = hook;
                                Node targetNode = GridManager.instance.GetNode(hit.point, currentHero.gridIndex);

                                if (targetNode != null)
                                    currentHero.PreviewPathToNode(targetNode, hook);

                                if (PathfinderMaster.instance.IsTargetNodeNeighbour(currentHero.CurrentNode, targetNode))
                                {
                                    currentHero.IsInteractionPointBlank = true;
                                }
                            }

                            if (currentHero.currentInteractionHook != null && hook == null)
                            {
                                currentHero.currentInteractionHook = null;
                                currentHero.IsInteractionPointBlank = false;
                            }
                        }
                    }
                }

                if (currentHero.currentInteractionHook != null && Input.GetKeyDown(KeyCode.Space))
                {
                    currentHero.IsInteractionInitialized = true;
                    currentHero.CreateInteractionContainer(currentHero.currentInteractionHook.interactionContainer);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (currentMouseLogic == null)
                    currentMouseLogic = selectMove;
            }

            HandleMouse();
        }
        void HandleMouse()
        {
            if (currentHero != null && (currentHero.IsInteracting || currentHero.currentInteractionHook != null))
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                if (currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, hit);
            }
        }
        public void OnSelectCurrentHero(HeroController hero)
        {
            if (heroIsMoving)
                return;

            if (currentHero == hero)
                return;

            if (hero != null)
            {
                currentHero = hero;
                UiManager.instance.OnHeroSelected(currentHero);

                if (currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, currentHero);
            }
        }
    }
}