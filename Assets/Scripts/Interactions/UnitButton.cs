using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HOMM_BM
{
    public class UnitButton : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI stackSize = default;
        [SerializeField]
        Image backgroundImage = default;
        [SerializeField]
        Image unitIcon = default;

        UnitController unitController;
        public UnitController UnitController { get => unitController; set => unitController = value; }

        private void Start()
        {
            unitIcon.sprite = unitController.UnitImage.sprite;
            stackSize.text = unitController.StackSize.ToString();

            if (unitController.gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
            {
                backgroundImage.sprite = UiManager.instance.EnemyBackgroundImage;
            }
            else
            {
                backgroundImage.sprite = UiManager.instance.FriendlyBackgroundImage;
            }
        }
    }
}