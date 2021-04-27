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
        public GameObject sliderPrefab;
        public GameObject heroSelectPrefab;

        public GameObject interactionPrefab;
        public Transform interactionsParent;

        public GameObject currentUnitPrefab;
        public GameObject unitPrefab;

        GameObject currentUnitUi;

        List<GameObject> unitsUiQueue = new List<GameObject>();

        Slider stepsSlider;
        public static UiManager instance;
        private void Awake()
        {
            instance = this;
        }

        public void AddStepsSlider(HeroController hero)
        {
            GameObject go = Instantiate(sliderPrefab);
            go.transform.SetParent(sliderPrefab.transform.parent);
            go.transform.localScale = Vector3.one;

            stepsSlider = go.GetComponentInChildren<Slider>();
            stepsSlider.maxValue = hero.stepsCount;
            stepsSlider.minValue = 0;
            stepsSlider.value = stepsSlider.maxValue;

            hero.InteractionSlider = stepsSlider;

            go.SetActive(true);
        }
        public void ResetInteractionSlider(HeroController hero)
        {
            hero.InteractionSlider.value = hero.InteractionSlider.maxValue;
        }

        public void CreateUiObjectForInteraction(InteractionInstance instance)
        {
            GameObject go = Instantiate(interactionPrefab);
            go.transform.SetParent(interactionsParent);
            go.transform.localScale = Vector3.one;
            go.SetActive(true);

            InteractionButton interactionButton = go.GetComponentInChildren<InteractionButton>();
            interactionButton.interactionInstance = instance;
            instance.uiObject = interactionButton;
        }
        public void AddHeroButton(HeroController heroController)
        {
            GameObject go = Instantiate(heroSelectPrefab);
            go.transform.SetParent(heroSelectPrefab.transform.parent);
            go.transform.localScale = Vector3.one;
            HeroSelectButton button = go.GetComponentInChildren<HeroSelectButton>();
            button.heroController = heroController;
            go.SetActive(true);
        }

        void AddCurrentUnitIcon(UnitController unitController)
        {
            GameObject go = Instantiate(currentUnitPrefab);
            go.transform.SetParent(currentUnitPrefab.transform.parent);
            go.transform.localScale = Vector3.one;
            UnitButton button = go.GetComponentInChildren<UnitButton>();
            button.unitController = unitController;
            go.SetActive(true);

            currentUnitUi = go;
        }
        void ClearCurrentUnitUi()
        {
            Destroy(currentUnitUi);
        }

        public void AddUnitIcons(List<UnitController> unitsQueue)
        {
            //Add something to differe enemy with friendly units
            foreach (UnitController unit in unitsQueue)
            {
                GameObject go = Instantiate(unitPrefab);
                go.transform.SetParent(unitPrefab.transform.parent);
                go.transform.localScale = Vector3.one;
                UnitButton button = go.GetComponentInChildren<UnitButton>();
                button.unitController = unit;
                go.SetActive(true);

                unitsUiQueue.Add(go);
            }
        }

        public void UpdateUnitIcons()
        {
            ClearCurrentUnitUi();

            GameObject newGo = Instantiate(unitPrefab);
            newGo.transform.SetParent(unitPrefab.transform.parent);
            newGo.transform.localScale = Vector3.one;
            UnitButton button = newGo.GetComponentInChildren<UnitButton>();
            button.unitController = BattleManager.instance.PreviousUnit;
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
                    if (button != null && button.unitController != null)
                    {
                        if (unit.Equals(button.unitController))
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
        }

        public void OnUnitTurn(List<UnitController> unitsQueue, UnitController currentUnit, bool isInitialize)
        {
            if (isInitialize)
            {
                AddUnitIcons(unitsQueue);
            }
            else
            {
                UpdateUnitIcons();
            }

            AddCurrentUnitIcon(currentUnit);

            BattleManager.instance.CalculatePath = true;
        }
    }
}