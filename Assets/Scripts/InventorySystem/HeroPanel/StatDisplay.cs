using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace HOMM_BM
{
    public class StatDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private HeroStat heroStat;
        public HeroStat Stat
        {
            get { return heroStat; }
            set
            {
                heroStat = value;
                UpdateStatValue();
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                nameText.text = name.ToUpper();
            }
        }

        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI valueText;
        [SerializeField] StatTooltip tooltip;

        private bool showingTooltip;

        private void OnValidate()
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            nameText = texts[0];
            valueText = texts[1];

            if (tooltip == null)
                tooltip = FindObjectOfType<StatTooltip>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltip.ShowTooltip(Stat, Name);
            showingTooltip = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tooltip.HideTooltip();
            showingTooltip = false;
        }

        public void UpdateStatValue()
        {
            valueText.text = heroStat.Value.ToString();
            if (showingTooltip)
            {
                tooltip.ShowTooltip(Stat, Name);
            }
        }
    }
}