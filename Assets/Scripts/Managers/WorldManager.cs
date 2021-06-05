using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using System.Linq;

namespace HOMM_BM
{
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager instance;

        List<HeroController> heroesQueue = new List<HeroController>();
        bool calculatingAiMove;

        public GameObject hitLookAtPrefab;
        public Camera MainCamera;

        [SerializeField]
        CinemachineVirtualCamera actionCamera = default;
        [SerializeField]
        CinemachineVirtualCamera panAndZoomCamera = default;

        public HeroController currentHero;
        public HeroController previousHero;

        [HideInInspector]
        public bool heroIsMoving;

        MouseLogicWorld currentMouseLogic;
        public MouseLogicWorld selectMove;

        public List<HeroController> HeroesQueue { get => heroesQueue; set => heroesQueue = value; }

        private void Awake()
        {
            instance = this;
        }
        public void Initialize()
        {
            UiManager.instance.DeactivateBattleUi();

            ActivatePanAndZoomCamera();

            HeroController[] heroes = FindObjectsOfType<HeroController>();
            foreach (HeroController hero in heroes)
            {
                Node node = GridManager.instance.GetNode(hero.transform.position, hero.GridIndex);
                hero.transform.position = node.worldPosition;

                if (hero.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                {
                    UiManager.instance.AddHeroButton(hero);
                    UiManager.instance.AddStepsSlider(hero);
                }
                else
                {
                    UiManager.instance.AddStepsSliderForAi(hero);
                }
            }

            InteractionHook[] gridObjects = FindObjectsOfType<InteractionHook>();
            foreach (InteractionHook go in gridObjects)
            {
                Node node = GridManager.instance.GetNode(go.transform.position, go.gridIndex);
                go.transform.position = node.worldPosition;
            }

            UiManager.instance.ActivateWorldUi();

            if (heroes.Length != 0)
            {
                heroesQueue = new List<HeroController>(heroes);
                currentHero = heroesQueue.First();

                OnCurrentHeroTurn();
            }
        }

        float timer;
        bool waitForNextTurn;
        private void Update()
        {
            if (!GameManager.instance.CurrentGameState.Equals(GameState.WORLD))
                return;

            if (waitForNextTurn)
            {
                timer += Time.deltaTime;

                OnMoveFinished();

                return;
            }

            if (currentHero != null)
            {
                //Temporary, making sure friendly hero move ends (atm as soon as action points are spent)
                if (currentHero.InteractionSlider != null && currentHero.InteractionSlider.value == 0)
                {
                    Debug.Log("Action points spent!");
                    OnMoveFinished();
                    return;
                }

                if (currentHero.IsInteracting || currentHero.IsInteractionInitialized)
                    return;

                if (currentHero.gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
                {
                    if (WorldSimulationManager.instance.AiInteracting || heroIsMoving)
                        return;

                    if (!calculatingAiMove)
                    {
                        calculatingAiMove = true;
                        WorldSimulationManager.instance.Initialize();
                        return;
                    }
                }
                else
                {
                    HandleMouseClickAction();

                    if (currentHero.currentInteractionHook != null && GameManager.instance.Keyboard.spaceKey.isPressed)
                    {
                        ActivateLookAtActionCamera(currentHero.currentInteractionHook.transform);

                        currentHero.IsInteractionInitialized = true;

                        if (currentHero.ActionPoints < PathfinderMaster.instance.StoredPath.Count)
                        {
                            currentHero.InitializeMoveToInteractionContainer(PathfinderMaster.instance.NodeAtMaxRange);
                        }
                        else
                        {
                            currentHero.CreateInteractionContainer(currentHero.currentInteractionHook.interactionContainer);
                        }
                    }
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
                Ray ray = Camera.main.ScreenPointToRay(GameManager.instance.Mouse.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, 100))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        InteractionHook hook = hit.transform.GetComponentInParent<InteractionHook>();

                        if (hook != null)
                        {
                            if (hook.GetComponentInParent<HeroController>() != currentHero)
                            {
                                currentHero.currentInteractionHook = hook;
                                Node targetNode = GridManager.instance.GetNode(hit.point, currentHero.GridIndex);

                                if (targetNode != null)
                                {
                                    currentHero.PreviewPathToNode(targetNode, hook);
                                    PathfinderMaster.instance.CreatedNodes.Add(targetNode);
                                }

                                if (PathfinderMaster.instance.IsTargetNodeNeighbour(currentHero.CurrentNode, targetNode))
                                {
                                    currentHero.IsInteractionPointBlank = true;
                                }
                            }
                        }

                        if (!currentHero.ReallyEnterTheBattlePrompt.gameObject.activeSelf && currentHero.currentInteractionHook != null && hook == null)
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

            //if (currentHero != null)
            //{
            //    panAndZoomCamera.transform.position = actionCamera.transform.position;
            //}
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

            Ray ray = Camera.main.ScreenPointToRay(GameManager.instance.Mouse.position.ReadValue());
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
            ActivatePanAndZoomCamera();

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

        public void OnMoveFinished()
        {
            if (timer <= 1)
            {
                if(!waitForNextTurn)
                    waitForNextTurn = true;
                return;
            }

            waitForNextTurn = false;
            timer = 0;

            if (currentHero.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                UiManager.instance.DeselectHero(currentHero);

            heroesQueue.RemoveAt(0);
            heroesQueue.Add(currentHero);

            previousHero = currentHero;

            if (heroesQueue.First().gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
                currentHero = heroesQueue.First();
            else
            {
                HeroController hero = heroesQueue.First();
                UiManager.instance.ResetInteractionSlider(hero);
                hero.ActionPoints = hero.StepsCount;

                currentHero = null;
            }

            OnCurrentHeroTurn();
        }
        public void OnCurrentHeroTurn()
        {
            if (WorldSimulationManager.instance.AiInteracting)
                WorldSimulationManager.instance.AiInteracting = false;

            if (calculatingAiMove)
                calculatingAiMove = false;

            if (currentHero != null)
            {
                if (currentHero.gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
                {
                    GameReferencesManager.instance.WorldAudio.Stop();

                    UiManager.instance.ShowEnemyTurnDisplay(currentHero);
                    GameReferencesManager.instance.EnemyTurnAudio.Play();
                }

                UiManager.instance.ResetInteractionSlider(currentHero);
                currentHero.ActionPoints = currentHero.StepsCount;

                if (previousHero != null && previousHero.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                    InitializeHeroInteractionHook(previousHero);
            }
            else
            {
                UiManager.instance.HideEnemyTurnDisplay();
                GameReferencesManager.instance.EnemyTurnAudio.Stop();

                GameReferencesManager.instance.WorldAudio.Play();
            }
        }

        void InitializeHeroInteractionHook(HeroController hero)
        {
            HeroInteractionHook hook = hero.GetComponentInChildren<HeroInteractionHook>();
            if (hook == null)
                Debug.Log("Hook is null!");

            hook.ItemSlots.Clear();
            foreach (ItemSlot slot in hero.InventoryReference.Inventory.ItemSlots)
            {
                if (slot.Item == null)
                    continue;

                if (slot.Item.GetType() == typeof(UnitItem))
                {
                    hook.ItemSlots.Add(slot, slot.Amount);
                }
            }
        }
    }
}