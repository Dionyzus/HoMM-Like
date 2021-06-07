using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class UiManager : MonoBehaviour
    {
        [SerializeField]
        GameObject battleUi = default;
        [SerializeField]
        GameObject worldUi = default;

        public GameObject SliderPrefab;
        public GameObject HeroSelectPrefab;

        public GameObject InteractionPrefab;
        public Transform InteractionsParent;

        public GameObject CurrentUnitPrefab;
        public GameObject UnitPrefab;

        GameObject currentUnitUi;

        List<GameObject> unitsUiQueue = new List<GameObject>();

        [SerializeField]
        Sprite friendlyBackgroundImage;
        [SerializeField]
        Sprite enemyBackgroundImage;

        Slider stepsSlider;
        public static UiManager instance;

        public Sprite FriendlyBackgroundImage { get => friendlyBackgroundImage; set => friendlyBackgroundImage = value; }
        public Sprite EnemyBackgroundImage { get => enemyBackgroundImage; set => enemyBackgroundImage = value; }

        public GameObject InventoryPrefab;

        public GameObject enemyTurn;
        public Slider currentEnemyTurnSlider;

        public GameObject ObtainedItemPrefab;
        public Transform ObtainedItemParent;

        private void Awake()
        {
            instance = this;
        }

        public void AddHeroInventory(HeroController hero)
        {
            GameObject go = Instantiate(InventoryPrefab);
            go.transform.SetParent(this.transform);
            go.transform.SetAsFirstSibling();
            go.transform.localScale = Vector3.one;
            RectTransform transform = go.transform.GetComponentInChildren<RectTransform>();
            transform.localPosition = Vector3.zero;

            go.gameObject.SetActive(true);

            hero.InitializeInventory(go.GetComponent<InventoryReference>());
        }
        public void ActivateBattleUi()
        {
            battleUi.gameObject.SetActive(true);
        }
        public void DeactivateBattleUi()
        {
            battleUi.gameObject.SetActive(false);
        }
        public void ActivateWorldUi()
        {
            worldUi.gameObject.SetActive(true);
        }
        public void DeactivateWorldUi()
        {
            worldUi.gameObject.SetActive(false);
        }

        public void AddStepsSlider(HeroController hero)
        {
            GameObject go = Instantiate(SliderPrefab);
            go.transform.SetParent(SliderPrefab.transform.parent);
            go.transform.localScale = Vector3.one;

            stepsSlider = go.GetComponentInChildren<Slider>();
            stepsSlider.maxValue = hero.StepsCount;
            stepsSlider.minValue = 0;
            stepsSlider.value = stepsSlider.maxValue;

            hero.InteractionSlider = stepsSlider;

            go.SetActive(true);
        }
        public void AddStepsSliderForAi(HeroController hero)
        {
            GameObject go = Instantiate(SliderPrefab);

            go.transform.SetParent(hero.transform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;

            Slider stepsSlider = go.GetComponentInChildren<Slider>();

            stepsSlider.maxValue = hero.StepsCount;
            stepsSlider.minValue = 0;
            stepsSlider.value = stepsSlider.maxValue;

            hero.InteractionSlider = stepsSlider;
        }
        public void ResetInteractionSlider(HeroController hero)
        {
            hero.InteractionSlider.value = hero.InteractionSlider.maxValue;
        }

        public void CreateUiObjectForInteraction(InteractionInstance instance, Sprite sprite)
        {
            GameObject go = Instantiate(InteractionPrefab);
            go.transform.SetParent(InteractionsParent);
            go.transform.localScale = Vector3.one;
            go.SetActive(true);

            InteractionButton interactionButton = go.GetComponentInChildren<InteractionButton>();
            interactionButton.interactionInstance = instance;

            Image image = interactionButton.GetComponentInChildren<Image>();
            image.sprite = sprite;

            instance.uiObject = interactionButton;
        }

        public void DisplayObtainedItem(InteractionHook hook)
        {
            GameObject go = Instantiate(ObtainedItemPrefab);
            go.transform.SetParent(ObtainedItemParent);
            go.transform.localScale = Vector3.one;
            RectTransform transform = go.transform.GetComponentInChildren<RectTransform>();
            transform.localPosition = Vector3.zero;

            ObtainedItemDisplay obtainedItemDisplay = go.GetComponentInChildren<ObtainedItemDisplay>();
            obtainedItemDisplay.ItemIcon.sprite = hook.Item.Icon;

            if (hook.Amount > 1)
                obtainedItemDisplay.AmountText.text = hook.Amount.ToString();
            else
                obtainedItemDisplay.AmountText.text = "";

            go.SetActive(true);
        }

        public void AddHeroButton(HeroController heroController)
        {
            GameObject go = Instantiate(HeroSelectPrefab);
            go.transform.SetParent(HeroSelectPrefab.transform.parent);
            go.transform.localScale = Vector3.one;
            HeroSelectButton button = go.GetComponentInChildren<HeroSelectButton>();
            button.heroController = heroController;
            go.SetActive(true);
        }

        void AddCurrentUnitIcon(UnitController unitController)
        {
            GameObject go = Instantiate(CurrentUnitPrefab);
            go.transform.SetParent(CurrentUnitPrefab.transform.parent);
            go.transform.localScale = Vector3.one;
            UnitButton button = go.GetComponentInChildren<UnitButton>();
            button.UnitController = unitController;
            go.SetActive(true);

            currentUnitUi = go;
        }
        void ClearCurrentUnitUi()
        {
            Destroy(currentUnitUi);
        }

        public void AddUnitIcons(List<UnitController> unitsQueue)
        {
            foreach (UnitController unit in unitsQueue)
            {
                GameObject go = Instantiate(UnitPrefab);
                go.transform.SetParent(UnitPrefab.transform.parent);
                go.transform.localScale = Vector3.one;
                UnitButton button = go.GetComponentInChildren<UnitButton>();
                button.UnitController = unit;
                go.SetActive(true);

                unitsUiQueue.Add(go);
            }
        }

        public void UpdateUnitIcons()
        {
            ClearCurrentUnitUi();

            GameObject newGo = Instantiate(UnitPrefab);
            newGo.transform.SetParent(UnitPrefab.transform.parent);
            newGo.transform.localScale = Vector3.one;
            UnitButton button = newGo.GetComponentInChildren<UnitButton>();
            button.UnitController = BattleManager.instance.PreviousUnit;
            newGo.SetActive(true);

            GameObject go = unitsUiQueue.ElementAt(0);
            Destroy(go);

            unitsUiQueue.RemoveAt(0);

            unitsUiQueue.Add(newGo);
        }

        public void UpdateUiOnUnitDeath(UnitController unit)
        {
            if (unit != null)
            {
                foreach (GameObject unitUi in unitsUiQueue)
                {
                    UnitButton button = unitUi.GetComponentInChildren<UnitButton>();
                    if (button != null && button.UnitController != null)
                    {
                        if (unit.Equals(button.UnitController))
                        {
                            unitsUiQueue.Remove(unitUi);
                            Destroy(unitUi);
                            break;
                        }
                    }
                }
            }
        }

        public void OnHeroSelected(HeroController targetHero)
        {
            HeroSelectButton[] characterDisplays = FindObjectsOfType<HeroSelectButton>();

            //Better way to do this?
            foreach (HeroSelectButton displays in characterDisplays)
            {
                Outline outline = displays.GetComponentInChildren<Outline>();

                if (outline.enabled == true)
                {
                    outline.enabled = false;
                }

                if (displays.heroController == targetHero)
                {
                    outline.enabled = true;
                }
            }

            if (targetHero.InventoryReference == null)
                AddHeroInventory(targetHero);
        }

        public void DeselectHero(HeroController heroController)
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
        }

        public void OnUnitTurn(List<UnitController> unitsQueue, UnitController currentUnit, bool isInitialize)
        {
            if (isInitialize)
            {
                AddUnitIcons(unitsQueue);
            }
            else
            {
                UpdateStackSizeUi();
                UpdateUnitIcons();
            }

            AddCurrentUnitIcon(currentUnit);

            BattleManager.instance.CalculatePath = true;
        }

        public void ShowEnemyTurnDisplay(HeroController heroController)
        {
            enemyTurn.SetActive(true);
            currentEnemyTurnSlider.maxValue = heroController.StepsCount;
            currentEnemyTurnSlider.minValue = 0;
            currentEnemyTurnSlider.value = 0;
        }

        public void HideEnemyTurnDisplay()
        {
            enemyTurn.SetActive(false);
        }

        void UpdateStackSizeUi()
        {
            foreach (GameObject unit in unitsUiQueue)
            {
                UnitButton button = unit.GetComponentInChildren<UnitButton>();
                button.UpdateStackSizeText();
            }
        }

        public void CleanBattleUiData()
        {
            ClearCurrentUnitUi();

            foreach (GameObject unitUi in unitsUiQueue)
            {
                Destroy(unitUi);
            }

            unitsUiQueue.Clear();
            currentUnitUi = null;
        }
    }
}