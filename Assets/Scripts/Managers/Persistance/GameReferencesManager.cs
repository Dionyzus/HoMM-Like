using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class GameReferencesManager : MonoBehaviour
    {
        public static GameReferencesManager instance;

        [SerializeField]
        DialogPrompt dialogPrompt = default;
        [SerializeField]
        SplitArmyPrompt splitArmyPrompt = default;

        HeroController heroController;
        SimpleHero simpleHero;

        UnitItem interactionUnit;
        int stackSize;
        int interactionStackSize;

        private const string INTERACTIONS = "Interactions";
        public SimpleHero SimpleHero { get => simpleHero; set => simpleHero = value; }
        public int InteractionStackSize { get => interactionStackSize; set => interactionStackSize = value; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        public void PrepareInteractionUnit(UnitItem unitItem, int stackSize)
        {
            interactionUnit = unitItem;
            interactionStackSize = stackSize;
            this.stackSize = stackSize;
        }
        public void LoadTargetScene(string sceneName)
        {
            StartCoroutine(LoadScene(sceneName));
        }

        IEnumerator LoadScene(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!operation.isDone)
            {
                yield return null;
            }

            GameManager.instance.StateInitialized = false;

            if (GameManager.instance.CurrentGameState == GameState.WORLD && heroController != null)
            {
                if (simpleHero != null)
                {
                    Destroy(simpleHero.gameObject);
                    simpleHero = null;
                }

                UpdateSceneStateHandler();

                heroController.gameObject.SetActive(true);

                UiManager.instance.DeactivateBattleUi();
                UiManager.instance.ActivateWorldUi();
            }

            if (GameManager.instance.CurrentGameState == GameState.WORLD && heroController == null)
            {
                InitializeHeroController();
                InitializeSceneStateHandler();
            }
            if (GameManager.instance.CurrentGameState == GameState.BATTLE)
            {
                SceneStateHandler.instance.InteractionHooks.Clear();

                GameManager.instance.WorldInitialized = true;

                InitializeSimpleHero();
                InitializeCurrentHeroInventoryUnits();

                if (stackSize >= (int)StackDescription.LARGE)
                {
                    InitializeInteractionUnits((int)StackSplit.MAXIMAL);
                }
                else if (stackSize > (int)StackDescription.NORMAL && stackSize < (int)StackDescription.LARGE)
                {
                    InitializeInteractionUnits((int)StackSplit.REGULAR);
                }
                else
                {
                    InitializeInteractionUnits((int)StackSplit.MINIMAL);
                }
            }
        }
        void InitializeSceneStateHandler()
        {
            GameObject interactionsParent = GameObject.FindWithTag(INTERACTIONS);
            InteractionHook[] interactions = interactionsParent.GetComponentsInChildren<InteractionHook>();

            SetActivateStateForInteractions(interactions);
        }

        void UpdateSceneStateHandler()
        {
            GameObject interactionsParent = GameObject.FindWithTag(INTERACTIONS);
            InteractionHook[] interactions = interactionsParent.GetComponentsInChildren<InteractionHook>();

            UpdateActiveStateForInteractions(interactions);
        }
        void UpdateActiveStateForInteractions(InteractionHook[] interactions)
        {
            foreach (InteractionHook hook in interactions)
            {
                bool interactionActive = SceneStateHandler.instance.GetActiveState(hook.transform.name);
                if (!interactionActive)
                    hook.gameObject.SetActive(false);
                else
                    SceneStateHandler.instance.InteractionHooks.Add(hook);
            }
        }

        void SetActivateStateForInteractions(InteractionHook[] interactions)
        {
            foreach (InteractionHook hook in interactions)
            {
                SceneStateHandler.instance.InteractionHooks.Add(hook);
                SceneStateHandler.instance.SetActiveState(hook.transform.name, true);
            }
        }
        void InitializeHeroController()
        {
            if (simpleHero != null)
            {
                Destroy(simpleHero.gameObject);
                simpleHero = null;
            }

            GameObject heroControllerGo = Instantiate(ResourcesManager.Instance.heroController);
            heroController = heroControllerGo.GetComponentInChildren<HeroController>();

            heroController.transform.position = ResourcesManager.Instance.heroControllerSpawnPosition.position;
            heroController.transform.rotation = ResourcesManager.Instance.heroControllerSpawnPosition.rotation;

            heroController.ReallyEnterTheBattlePrompt = dialogPrompt;
            heroController.SplitArmyPrompt = splitArmyPrompt;

            heroController.transform.SetParent(this.transform);
            heroController.gameObject.SetActive(true);
        }
        void InitializeSimpleHero()
        {
            GameObject simpleHeroGo = Instantiate(ResourcesManager.Instance.heroSimple);
            simpleHero = simpleHeroGo.GetComponentInChildren<SimpleHero>();

            simpleHero.Initialize();

            HideHeroData();

            simpleHero.transform.position = ResourcesManager.Instance.simpleHeroSpawnPosition.position;
            simpleHero.transform.rotation = ResourcesManager.Instance.simpleHeroSpawnPosition.rotation;

            simpleHero.transform.SetParent(this.transform);
            simpleHero.gameObject.SetActive(true);
        }

        void InitializeCurrentHeroInventoryUnits()
        {
            foreach (ItemSlot slot in simpleHero.inventoryReference.Inventory.ItemSlots)
            {
                if (slot.Item == null)
                    continue;

                if (slot.Item.GetType().Equals(typeof(UnitItem)))
                {
                    UnitItem unitItem = (UnitItem)slot.Item;
                    InstantiateUnit(unitItem, slot.Amount);
                }
            }
        }
        void InitializeInteractionUnits(int divider)
        {
            if (stackSize <= (int)StackDescription.NORMAL)
            {
                InstantiateStack(stackSize);
                return;
            }
            int newStackSize = Mathf.FloorToInt(stackSize / divider);
            InstantiateStack(newStackSize);

            stackSize -= newStackSize;

            if (stackSize >= (int)StackDescription.LARGE)
            {
                InitializeInteractionUnits((int)StackSplit.MAXIMAL);
            }
            else if (stackSize > (int)StackDescription.NORMAL && stackSize < (int)StackDescription.LARGE)
            {
                InitializeInteractionUnits((int)StackSplit.REGULAR);
            }
            else
            {
                InitializeInteractionUnits((int)StackSplit.MINIMAL);
            }
        }

        private void InstantiateStack(int stackSize)
        {
            UnitController unitInstance = Instantiate(ResourcesManager.Instance.Units[interactionUnit.GetUnit()]);
            unitInstance.StackSize = stackSize;
            unitInstance.transform.localScale = Vector3.one;

            unitInstance.InitializeUnitHitPoints();
            unitInstance.gameObject.SetActive(true);
        }

        void InstantiateUnit(UnitItem unitItem, int amount)
        {
            UnitController unitInstance = Instantiate(ResourcesManager.Instance.Units[unitItem.GetUnit()]);
            unitInstance.UnitType = unitItem.UnitType;
            unitInstance.StackSize = amount;
            unitInstance.transform.localScale = Vector3.one;

            unitInstance.InitializeUnitHitPoints();
            unitInstance.gameObject.SetActive(true);
        }

        void HideHeroData()
        {
            HeroSelectButton[] characterDisplays = FindObjectsOfType<HeroSelectButton>();

            foreach (HeroSelectButton displays in characterDisplays)
            {
                Outline outline = displays.GetComponentInChildren<Outline>();
                if (displays.heroController == heroController)
                {
                    outline.enabled = false;
                    break;
                }
            }

            heroController.gameObject.SetActive(false);
        }
    }
}