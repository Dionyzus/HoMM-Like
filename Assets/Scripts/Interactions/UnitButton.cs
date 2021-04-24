using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class UnitButton : MonoBehaviour
    {
        Button button;
        public UnitController unitController;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            button.GetComponentInChildren<Image>().sprite = unitController.UnitImage.sprite;
        }

        public void OnClick()
        {
            //Show unit stats, but probably should check how right click can be used

            //BattleManager.instance.OnCurrentUnitTurn(unitController);
        }
    }
}