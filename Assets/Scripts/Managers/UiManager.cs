using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class UiManager : MonoBehaviour
    {
        public GameObject sliderPrefab;
        public GameObject charSelectPrefab;

        public GameObject interactionPrefab;
        public Transform interactionsParent;

        Slider stepsSlider;
        public static UiManager instance;
        private void Awake()
        {
            instance = this;

            HeroController[] heroes = FindObjectsOfType<HeroController>();
            foreach (HeroController hero in heroes)
            {
                if (hero.gameObject.layer == GridManager.friendlyUnitsLayer)
                {
                    GameObject sliderParent = AddHeroButton(hero);
                    AddStepsSlider(hero, sliderParent);
                }

            }

            UnitController[] units = FindObjectsOfType<UnitController>();
            foreach (UnitController unit in units)
            {
                if (unit.gameObject.layer == GridManager.friendlyUnitsLayer)
                {
                    AddUnitIcon(unit);
                }
            }
        }

        public void AddStepsSlider(HeroController hero, GameObject sliderParent)
        {
            GameObject go = Instantiate(sliderPrefab);
            go.transform.SetParent(sliderParent.transform);
            go.transform.localScale = Vector3.one;

            stepsSlider = go.GetComponentInChildren<Slider>();
            stepsSlider.maxValue = hero.stepsCount;
            stepsSlider.minValue = 0;
            stepsSlider.value = stepsSlider.maxValue;

            hero.InteractionSlider = stepsSlider;

            go.SetActive(true);
        }
        public void ResetInteractionSlider(HeroController unit)
        {
            unit.InteractionSlider.value = unit.InteractionSlider.maxValue;
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
        public GameObject AddHeroButton(HeroController gridUnit)
        {
            GameObject go = Instantiate(charSelectPrefab);
            go.transform.SetParent(charSelectPrefab.transform.parent);
            go.transform.localScale = Vector3.one;
            HeroSelectButton button = go.GetComponentInChildren<HeroSelectButton>();
            button.heroController = gridUnit;
            go.SetActive(true);

            return go;
        }

        private void AddUnitIcon(UnitController unit)
        {
            Debug.Log("Unit icon: " + unit.gameObject.transform);
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

        public void OnUnitTurn(UnitController targetUnit)
        {
            Debug.Log("Current unit turn: " + targetUnit.transform);
            //CurrentUnitIcon[] characterDisplays = FindObjectsOfType<CurrentUnitIcon>();

            ////Better way to do this ?
            //foreach (CurrentUnitIcon displays in characterDisplays)
            //{
            //    Outline outline = displays.GetComponentInChildren<Outline>();

            //    if (outline.enabled == true)
            //    {
            //        outline.enabled = false;
            //    }

            //    if (displays.unitController == targetUnit)
            //    {
            //        outline.enabled = true;
            //    }
            //}
        }
    }
}