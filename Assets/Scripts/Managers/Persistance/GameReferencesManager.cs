﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class GameReferencesManager : MonoBehaviour
    {
        public static GameReferencesManager instance;

        HeroController heroController;
        SimpleHero simpleHero;

        GameObject sceneTriggerGo;

        UnitItem interactionUnit;
        int stackSize;

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
                if (sceneTriggerGo != null)
                    SceneStateHandler.instance.UpdateActiveState("SceneTrigger", sceneTriggerGo);

                heroController.gameObject.SetActive(true);

                UiManager.instance.DeactivateBattleUi();
                UiManager.instance.ActivateWorldUi();
            }

            if (GameManager.instance.CurrentGameState == GameState.WORLD && heroController == null)
            {
                if (sceneTriggerGo == null)
                {
                    sceneTriggerGo = Instantiate(ResourcesManager.Instance.sceneTrigger);
                    sceneTriggerGo.transform.position = ResourcesManager.Instance.sceneTriggerSpawnPosition.position;
                }

                SceneStateHandler.instance.SetActiveState("SceneTrigger", sceneTriggerGo, true);

                InitializeHeroController();
            }
            if (GameManager.instance.CurrentGameState == GameState.BATTLE && simpleHero == null)
            {

                GameManager.instance.WorldInitialized = true;

                InitializeSimpleHero();
                InitializeCurrentHeroInventoryUnits();

                InstantiateInteractionUnits();
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
        void InstantiateInteractionUnits()
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