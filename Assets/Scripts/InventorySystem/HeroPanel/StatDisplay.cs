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

        private string statName;
        public string StatName
        {
            get { return statName; }
            set
            {
                statName = value;
                nameText.text = statName.ToUpper();
            }
        }

        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI valueText;
        [SerializeField] StatTooltip statTooltip;

        private bool showingTooltip;

        public void Initialize()
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            nameText = texts[0];
            valueText = texts[1];

            if (statTooltip == null)
                statTooltip = FindObjectOfType<StatTooltip>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            statTooltip.ShowTooltip(Stat, StatName);
            showingTooltip = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            statTooltip.HideTooltip();
            showingTooltip = false;
        }

        public void UpdateStatValue()
        {
            valueText.text = heroStat.Value.ToString();
            if (showingTooltip)
            {
                statTooltip.ShowTooltip(Stat, StatName);
            }
        }
    }
}