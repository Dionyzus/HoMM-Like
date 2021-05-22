using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class HeroSelectButton : MonoBehaviour
    {
        Button button;
        public HeroController heroController;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            button.GetComponentInChildren<RawImage>().texture = heroController.renderTexture;
        }

        public void OnClick()
        {
            WorldManager.instance.OnSelectCurrentHero(heroController);
        }
    }
}
