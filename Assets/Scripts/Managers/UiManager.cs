using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class UiManager : MonoBehaviour
    {
        public GameObject sliderPrefab;
        Slider stepsSlider;
        public static UiManager instance;
        private void Awake()
        {
            instance = this;

            GridUnit[] gridUnits = FindObjectsOfType<GridUnit>();
            foreach (GridUnit unit in gridUnits)
            {
                if (unit.gameObject.layer == GridManager.friendlyUnitsLayer)
                {
                    GameObject sliderParent = AddCharacterButton(unit);
                    AddStepsSlider(unit, sliderParent);
                }

            }
        }
        public void AddStepsSlider(GridUnit unit, GameObject sliderParent)
        {
            GameObject go = Instantiate(sliderPrefab);
            go.transform.SetParent(sliderParent.transform);
            go.transform.localScale = Vector3.one;

            stepsSlider = go.GetComponentInChildren<Slider>();
            stepsSlider.maxValue = unit.stepsCountSlider;
            stepsSlider.minValue = 0;
            stepsSlider.value = stepsSlider.maxValue;

            unit.InteractionSlider = stepsSlider;

            go.SetActive(true);
        }
        public void ResetInteractionSlider(GridUnit unit)
        {
            unit.InteractionSlider.value = unit.InteractionSlider.maxValue;
        }

        public GameObject interactionPrefab;
        public Transform interactionsParent;

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

        public GameObject charSelectPrefab;
        public GameObject AddCharacterButton(GridUnit gridUnit)
        {
            GameObject go = Instantiate(charSelectPrefab);
            go.transform.SetParent(charSelectPrefab.transform.parent);
            go.transform.localScale = Vector3.one;
            UnitSelectButton button = go.GetComponentInChildren<UnitSelectButton>();
            button.gridUnit = gridUnit;
            go.SetActive(true);

            return go;
        }

        public void OnCharacterSelected(GridUnit targetUnit)
        {
            UnitSelectButton[] characterDisplays = FindObjectsOfType<UnitSelectButton>();

            //Better way to do this?
            foreach (UnitSelectButton displays in characterDisplays)
            {
                Outline outline = displays.GetComponentInChildren<Outline>();

                if (outline.enabled == true)
                {
                    outline.enabled = false;
                }

                if (displays.gridUnit == targetUnit)
                {
                    outline.enabled = true;
                }
            }
        }
    }
}