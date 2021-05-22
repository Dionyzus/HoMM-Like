using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HOMM_BM
{
    public class StatTooltip : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI StatNameText;
        [SerializeField] TextMeshProUGUI StatModifiersLabelText;
        [SerializeField] TextMeshProUGUI StatModifiersText;

        private readonly StringBuilder sb = new StringBuilder();

        public TextMeshProUGUI StatNameText1 { get => StatNameText; set => StatNameText = value; }
        public TextMeshProUGUI StatModifiersLabelText1 { get => StatModifiersLabelText; set => StatModifiersLabelText = value; }
        public TextMeshProUGUI StatModifiersText1 { get => StatModifiersText; set => StatModifiersText = value; }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void ShowTooltip(HeroStat stat, string statName)
        {
            StatNameText1.text = GetStatTopText(stat, statName);
            StatModifiersText1.text = GetStatModifiersText(stat);
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

            foreach (StatModifier mod in stat.StatModifiers)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                if (mod.Value > 0)
                    sb.Append("+");

                if (mod.Type == StatModType.Flat)
                {
                    sb.Append(mod.Value);
                }
                else
                {
                    sb.Append(mod.Value * 100);
                    sb.Append("%");
                }

                Item item = mod.Source as Item;

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