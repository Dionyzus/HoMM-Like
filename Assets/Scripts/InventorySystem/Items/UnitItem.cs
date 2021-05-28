using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Items/Unit Item")]
    public class UnitItem : Item
    {
        [SerializeField]
        UnitStats unitStats = default;

        public UnitAttackType UnitAttackType;
        public UnitType Unit;

        public string GetUnit()
        {
            return Unit.ToString().ToUpper();
        }

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

            sb.AppendLine();
            return sb.ToString();
        }

        private void AddStat(float value, string statName)
        {
            if (value != 0)
            {
                sb.Append(statName);
                sb.Append(" ");
                sb.Append(value);
                sb.AppendLine();
            }
        }
    }
}