using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HOMM_BM
{
    public class StatTooltip : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI statNameText;
        [SerializeField] TextMeshProUGUI statModifiersLabelText;
        [SerializeField] TextMeshProUGUI statModifiersText;

        private readonly StringBuilder sb = new StringBuilder();

        public TextMeshProUGUI StatNameText { get => statNameText; set => statNameText = value; }
        public TextMeshProUGUI StatModifiersLabelText { get => statModifiersLabelText; set => statModifiersLabelText = value; }
        public TextMeshProUGUI StatModifiersText { get => statModifiersText; set => statModifiersText = value; }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void ShowTooltip(HeroStat stat, string statName)
        {
            StatNameText.text = GetStatTopText(stat, statName);
            StatModifiersText.text = GetStatModifiersText(stat);
            gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }

        private string GetStatTopText(HeroStat stat, string statName)
        {
            sb.Length = 0;
            sb.Append(statName);
            sb.Append(" ");
            sb.Append(stat.Value);

            if (stat.Value != stat.BaseValue)
            {
                sb.Append(" (");
                sb.Append(stat.BaseValue);

                if (stat.Value > stat.BaseValue)
                    sb.Append("+");

                sb.Append(System.Math.Round(stat.Value - stat.BaseValue, 4));
                sb.Append(")");
            }

            return sb.ToString();
        }

        private string GetStatModifiersText(HeroStat stat)
        {
            sb.Length = 0;

            foreach (StatModifier modifier in stat.StatModifiers)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                if (modifier.Value > 0)
                    sb.Append("+");

                if (modifier.Type == StatModType.FLAT)
                {
                    sb.Append(modifier.Value);
                }
                else
                {
                    sb.Append(modifier.Value * 100);
                    sb.Append("%");
                }

                Item item = modifier.Source as Item;

                if (item != null)
                {
                    sb.Append(" ");
                    sb.Append(item.ItemName);
                }
                else
                {
                    Debug.LogError("Modifier is not an Item!");
                }
            }

            return sb.ToString();
        }
    }
}