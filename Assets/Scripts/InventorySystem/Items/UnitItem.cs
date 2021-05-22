using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Items/Unit Item")]
    public class UnitItem : Item
    {
        [SerializeField]
        UnitStats unitStats = default;
        [Space]
        public UnitAttackType UnitAttackType;

        public override Item GetCopy()
        {
            return Instantiate(this);
        }

        public override void Destroy()
        {
            Destroy(this);
        }

        public override string GetItemType()
        {
            return UnitAttackType.ToString();
        }

        public override string GetDescription()
        {
            sb.Length = 0;
            AddStat(unitStats.initiative, "Initiative");
            AddStat(unitStats.moral, "Moral");
            AddStat(unitStats.luck, "Luck");
            AddStat(unitStats.hitPoints, "Hit Points");
            AddStat(unitStats.damage, "Damage");
            AddStat(unitStats.attack, "Attack");
            AddStat(unitStats.defense, "Defense");

            return sb.ToString();
        }

        private void AddStat(float value, string statName, bool isPercent = false)
        {
            if (value != 0)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                if (value > 0)
                    sb.Append("+");

                else
                {
                    sb.Append(value);
                    sb.Append(" ");
                }
                sb.Append(statName);
            }
        }
    }
}