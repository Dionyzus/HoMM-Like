using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class UiManager : MonoBehaviour
    {
        public GameObject sliderPrefab;
        public Transform worldCanvasTransform;
        public static UiManager instance;
        private void Awake()
        {
            instance = this;

            GridUnit[] gridUnits = FindObjectsOfType<GridUnit>();
            foreach (GridUnit unit in gridUnits)
            {
                AddCharacterButton(unit);
            }
        }
        public GameObject GetDebugSlider()
        {
            GameObject go = Instantiate(sliderPrefab);
            go.transform.SetParent(worldCanvasTransform);
            return go;
        }
        public GameObject interactionPrefab;
        public Transform interactionsParent;

        public void SelectUnit(GridUnit gridUnit)
        {
            foreach (InteractionInstance instance in gridUnit.interactionInstances)
            {
                CreateUiObjectForInteraction(instance);
            }
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

        public GameObject charSelectPrefab;
        public void AddCharacterButton(GridUnit gridUnit)
        {
            GameObject go = Instantiate(charSelectPrefab);
            go.transform.SetParent(charSelectPrefab.transform.parent);
            go.transform.localScale = Vector3.one;
            UnitSelectButton button = go.GetComponentInChildren<UnitSelectButton>();
            button.gridUnit = gridUnit;
            go.SetActive(true);
        }
    }
}