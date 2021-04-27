using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;

namespace HOMM_BM
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager instance;

        public GameObject hitLookAtPrefab;
        public Camera MainCamera;

        [SerializeField]
        CinemachineVirtualCamera actionCamera = default;
        [SerializeField]
        CinemachineVirtualCamera panAndZoomCamera = default;

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
            ActivatePanAndZoomCamera();

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

                HandleMouseClickAction();

                if (currentHero.currentInteractionHook != null && GameManager.instance.Keyboard.spaceKey.isPressed)
                {
                    ActivateLookAtActionCamera(currentHero.currentInteractionHook.transform);

                    currentHero.IsInteractionInitialized = true;
                    currentHero.CreateInteractionContainer(currentHero.currentInteractionHook.interactionContainer);
                }
            }

            if (GameManager.instance.Mouse.leftButton.isPressed)
            {
                if (currentMouseLogic == null)
                    currentMouseLogic = selectMove;
            }

            HandleMouse();
        }

        private void HandleMouseClickAction()
        {
            if (GameManager.instance.Mouse.leftButton.isPressed)
            {
                Ray ray = MainCamera.ScreenPointToRay(GameManager.instance.Mouse.position.ReadValue());

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
        }

        public void ActivatePanAndZoomCamera()
        {
            panAndZoomCamera.Priority = 50;

            if (currentHero != null)
            {
                panAndZoomCamera.transform.position = actionCamera.transform.position;
            }
            //Should add castle position as default, atm fixed scene position
            panAndZoomCamera.transform.position = panAndZoomCamera.transform.position;
        }

        public void DeactivatePanAndZoomCamera()
        {
            panAndZoomCamera.Priority = 10;
        }

        public void ActivateLookAtActionCamera(Transform lookAtTarget)
        {
            DeactivatePanAndZoomCamera();

            actionCamera.Priority = 50;

            actionCamera.transform.position = panAndZoomCamera.transform.position;
            actionCamera.transform.rotation = panAndZoomCamera.transform.rotation;

            actionCamera.LookAt = lookAtTarget.transform;
        }

        public void DeactivateLookAtActionCamera()
        {
            actionCamera.Priority = 10;
            actionCamera.LookAt = null;

            ActivatePanAndZoomCamera();
        }

        void HandleMouse()
        {
            //Need to find another way to stop user from clicking while interaction is still ongoing
            if (currentHero != null && (currentHero.IsInteracting || currentHero.currentInteractionHook != null))
                return;

            Ray ray = MainCamera.ScreenPointToRay(GameManager.instance.Mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    if (currentMouseLogic != null)
                        currentMouseLogic.InteractTick(this, hit);
                }
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

                ActivatePanAndZoomCamera();

                if (currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, currentHero);
            }
        }
    }
}