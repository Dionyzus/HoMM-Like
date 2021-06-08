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
        private const string INTERACTIONS = "Interactions";

        Camera mainCamera;

        bool heroFight;
        bool enemyHeroDied;

        [SerializeField]
        AudioManager worldAudio = default;
        [SerializeField]
        AudioManager enemyTurnAudio = default;
        [SerializeField]
        AudioManager battleAudio = default;

        [SerializeField]
        GameObject loadingScreen = default;
        [SerializeField]
        DialogPrompt dialogPrompt = default;
        [SerializeField]
        SplitArmyPrompt splitArmyPrompt = default;

        HeroController heroController;
        HeroController enemyHero;

        SimpleHero simpleHero;
        SimpleHero enemySimpleHero;

        UnitItem interactionUnit;
        int stackSize;
        int interactionStackSize;

        Dictionary<int, int> slotUnits = new Dictionary<int, int>();

        List<HeroController> heroesQueue = new List<HeroController>();

        public SimpleHero SimpleHero { get => simpleHero; set => simpleHero = value; }
        public int InteractionStackSize { get => interactionStackSize; set => interactionStackSize = value; }
        public Dictionary<int, int> SlotUnits { get => slotUnits; set => slotUnits = value; }
        public SimpleHero EnemySimpleHero { get => enemySimpleHero; set => enemySimpleHero = value; }
        public HeroController EnemyHero { get => enemyHero; set => enemyHero = value; }
        public bool EnemyHeroDied { get => enemyHeroDied; set => enemyHeroDied = value; }
        public bool HeroFight { get => heroFight; set => heroFight = value; }
        public List<HeroController> HeroesQueue { get => heroesQueue; set => heroesQueue = value; }
        public AudioManager EnemyTurnAudio { get => enemyTurnAudio; set => enemyTurnAudio = value; }
        public AudioManager WorldAudio { get => worldAudio; set => worldAudio = value; }
        public Camera MainCamera { get => mainCamera; set => mainCamera = value; }

        ItemAmountDictionary items = new ItemAmountDictionary();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                mainCamera = Camera.main;
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
        int stackSizeMultiplier = 10;
        public void PrepareInteractionWithHero(ItemAmountDictionary items)
        {
            this.items = items;
            interactionStackSize = items.Count * stackSizeMultiplier;
        }
        public void LoadTargetScene(string sceneName)
        {
            StartCoroutine(LoadScene(sceneName));
        }

        IEnumerator LoadScene(string sceneName)
        {
            loadingScreen.SetActive(true);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!operation.isDone)
            {
                yield return null;
            }

            GameManager.instance.StateInitialized = false;

            if (GameManager.instance.CurrentGameState == GameState.WORLD && heroController != null)
            {
                battleAudio.Stop();
                worldAudio.Play();

                if (simpleHero != null)
                {
                    Destroy(simpleHero.gameObject);
                    simpleHero = null;
                }
                if (enemySimpleHero != null)
                {
                    Destroy(enemySimpleHero.gameObject);
                    enemySimpleHero = null;
                }

                UpdateSceneStateHandler();

                PathfinderMaster.instance.pathLineInRange.gameObject.SetActive(true);
                PathfinderMaster.instance.pathLineOutsideRange.gameObject.SetActive(true);

                heroController.gameObject.SetActive(true);
                heroController.AudioListener.enabled = true;

                if (enemyHeroDied)
                    DestroyEnemyHeroInstance();
                else
                    enemyHero.gameObject.SetActive(true);

                WorldManager.instance.HeroesQueue = heroesQueue;

                UiManager.instance.DeactivateBattleUi();
                UiManager.instance.ActivateWorldUi();

                mainCamera = WorldManager.instance.MainCamera;

                CursorManager.instance.SetToDefault();
            }

            if (GameManager.instance.CurrentGameState == GameState.WORLD && heroController == null)
            {
                battleAudio.Stop();
                worldAudio.Play();

                InitializeHeroController();
                InitializeEnemyHeroController();

                InitializeSceneStateHandler();

                mainCamera = WorldManager.instance.MainCamera;

                CursorManager.instance.SetToDefault();
            }
            if (GameManager.instance.CurrentGameState == GameState.BATTLE)
            {
                if (UiManager.instance.enemyTurn.gameObject.activeSelf)
                    UiManager.instance.HideEnemyTurnDisplay();

                enemyTurnAudio.Stop();
                worldAudio.Stop();
                battleAudio.Play();

                heroesQueue = WorldManager.instance.HeroesQueue;

                PathfinderMaster.instance.pathLineInRange.gameObject.SetActive(false);
                PathfinderMaster.instance.pathLineOutsideRange.gameObject.SetActive(false);

                SceneStateHandler.instance.InteractionHooks.Clear();

                GameManager.instance.WorldInitialized = true;

                InitializeSimpleHero();
                InitializeCurrentHeroInventoryUnits();

                enemyHero.gameObject.SetActive(false);

                if (items.Count != 0)
                {
                    heroFight = true;
                    InitializeEnemySimpleHero();
                    InitializeEnemyHeroUnits();
                }
                else
                {
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

                mainCamera = BattleManager.instance.MainCamera;

                CursorManager.instance.SetToDefault();
            }

            loadingScreen.SetActive(false);
        }

        void DestroyEnemyHeroInstance()
        {
            heroesQueue.Remove(enemyHero);

            Destroy(enemyHero.gameObject);
            enemyHero = null;
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

            if (heroController.AudioListener == null)
            {
                heroController.AudioListener = heroController.gameObject.AddComponent<AudioListener>();
                heroController.AudioListener.enabled = true;
            }
        }
        void InitializeEnemyHeroController()
        {
            GameObject heroControllerGo = Instantiate(ResourcesManager.Instance.enemyHeroController);
            enemyHero = heroControllerGo.GetComponentInChildren<HeroController>();

            enemyHero.transform.position = ResourcesManager.Instance.enemyHeroSpawnPosition.position;
            enemyHero.transform.rotation = ResourcesManager.Instance.enemyHeroSpawnPosition.rotation;

            enemyHero.transform.SetParent(this.transform);
            enemyHero.gameObject.SetActive(true);
        }
        void InitializeSimpleHero()
        {
            GameObject simpleHeroGo = Instantiate(ResourcesManager.Instance.heroSimple);
            simpleHero = simpleHeroGo.GetComponentInChildren<SimpleHero>();

            simpleHero.InitializeFriendlyHero(heroController);

            HideHeroData();

            simpleHero.transform.position = ResourcesManager.Instance.simpleHeroSpawnPosition.position;
            simpleHero.transform.rotation = ResourcesManager.Instance.simpleHeroSpawnPosition.rotation;

            simpleHero.transform.SetParent(this.transform);
            simpleHero.gameObject.SetActive(true);
        }
        void InitializeEnemySimpleHero()
        {
            GameObject simpleHeroGo = Instantiate(ResourcesManager.Instance.enemyHeroSimple);
            enemySimpleHero = simpleHeroGo.GetComponentInChildren<SimpleHero>();

            enemySimpleHero.InitializeEnemyHero(enemyHero);

            enemySimpleHero.transform.position = ResourcesManager.Instance.enemyHeroSimpleSpawnPosition.position;
            enemySimpleHero.transform.rotation = ResourcesManager.Instance.enemyHeroSimpleSpawnPosition.rotation;

            enemySimpleHero.transform.SetParent(this.transform);
            enemySimpleHero.gameObject.SetActive(true);
        }

        void InitializeCurrentHeroInventoryUnits()
        {
            slotUnits.Clear();

            foreach (ItemSlot slot in simpleHero.inventoryReference.Inventory.ItemSlots)
            {
                if (slot.Item == null)
                    continue;

                if (slot.Item.GetType().Equals(typeof(UnitItem)))
                {
                    UnitItem unitItem = (UnitItem)slot.Item;
                    InstantiateFriendlyStack(unitItem, slot);
                }
            }
        }
        void InitializeEnemyHeroUnits()
        {
            foreach (KeyValuePair<Item, int> entry in items)
            {
                UnitItem unitItem = (UnitItem)entry.Key;

                InstantiateStack(unitItem, entry.Value);
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
        private void InstantiateStack(UnitItem unitItem, int stackSize)
        {
            UnitController unitInstance = Instantiate(ResourcesManager.Instance.Units[unitItem.GetUnit()]);
            unitInstance.StackSize = stackSize;
            unitInstance.transform.localScale = Vector3.one;
            unitInstance.transform.rotation = enemySimpleHero.transform.rotation;

            unitInstance.InitializeUnitHitPoints();
            unitInstance.gameObject.SetActive(true);
        }
        private void InstantiateStack(int stackSize)
        {
            UnitController unitInstance = Instantiate(ResourcesManager.Instance.Units[interactionUnit.GetUnit()]);
            unitInstance.StackSize = stackSize;
            unitInstance.transform.localScale = Vector3.one;
            unitInstance.transform.eulerAngles = new Vector3(0, 180, 0);

            unitInstance.InitializeUnitHitPoints();
            unitInstance.gameObject.SetActive(true);
        }

        void InstantiateFriendlyStack(UnitItem unitItem, ItemSlot itemSlot)
        {
            UnitController unitInstance = Instantiate(ResourcesManager.Instance.Units[unitItem.GetUnit()]);
            unitInstance.UnitType = unitItem.UnitType;
            unitInstance.StackSize = itemSlot.Amount;
            unitInstance.transform.localScale = Vector3.one;
            unitInstance.transform.rotation = simpleHero.transform.rotation;

            unitInstance.InitializeUnitHitPoints();
            unitInstance.gameObject.SetActive(true);

            slotUnits.Add(itemSlot.GetInstanceID(), unitInstance.GetInstanceID());
        }

        public void HideHeroData()
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

            heroController.AudioListener.enabled = true;
            heroController.gameObject.SetActive(false);
        }
    }
}