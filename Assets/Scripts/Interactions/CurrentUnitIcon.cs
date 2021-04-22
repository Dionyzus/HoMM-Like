using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class CurrentUnitIcon : MonoBehaviour
    {
        Button button;
        public UnitController unitController;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            button.GetComponentInChildren<RawImage>().texture = unitController.unitImage;
        }

        public void OnClick()
        {
            GameManager.BattleManager.OnCurrentUnitTurn(unitController);
        }
    }
}