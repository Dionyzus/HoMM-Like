using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class UnitSelectButton : MonoBehaviour
    {
        Button button;
        public GridUnit gridUnit;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            button.GetComponentInChildren<RawImage>().texture = gridUnit.unitImage;
        }

        public void OnClick()
        {
            GameManager.instance.OnSelectCurrentUnit(gridUnit);
        }
    }
}
