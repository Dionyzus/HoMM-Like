using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Items/Equippable Item")]
    public class EquippableItem : Item
    {
        public int LogisticsBonus;
        public int LuckBonus;
        public int AttackBonus;
        public int DefenseBonus;
        [Space]
        public float LogisticsPercentBonus;
        public float LuckPercentBonus;
        public float AttackPercentBonus;
        public float DefensePercentBonus;
        [Space]
        public ArtifactType ArtifactType;

        public override Item GetCopy()
        {
            return Instantiate(this);
        }

        public override void Destroy()
        {
            Destroy(this);
        }

        public void Equip(HeroController hero)
        {
            if (LogisticsBonus != 0)
                hero.Logistics.AddModifier(new StatModifier(LogisticsBonus, StatModType.FLAT, this));
            if (LuckBonus != 0)
                hero.Luck.AddModifier(new StatModifier(LuckBonus, StatModType.FLAT, this));
            if (AttackBonus != 0)
                hero.Attack.AddModifier(new StatModifier(AttackBonus, StatModType.FLAT, this));
            if (DefenseBonus != 0)
                hero.Defense.AddModifier(new StatModifier(DefenseBonus, StatModType.FLAT, this));

            if (LogisticsPercentBonus != 0)
                hero.Logistics.AddModifier(new StatModifier(LogisticsPercentBonus, StatModType.PERCENT_MULT, this));
            if (LuckPercentBonus != 0)
                hero.Luck.AddModifier(new StatModifier(LuckPercentBonus, StatModType.PERCENT_MULT, this));
            if (AttackPercentBonus != 0)
                hero.Attack.AddModifier(new StatModifier(AttackPercentBonus, StatModType.PERCENT_MULT, this));
            if (DefensePercentBonus != 0)
                hero.Defense.AddModifier(new StatModifier(DefensePercentBonus, StatModType.PERCENT_MULT, this));
        }

        public void Unequip(HeroController hero)
        {
            hero.Logistics.RemoveAllModifiersFromSource(this);
            hero.Luck.RemoveAllModifiersFromSource(this);
            hero.Attack.RemoveAllModifiersFromSource(this);
            hero.Defense.RemoveAllModifiersFromSource(this);
        }

        public override string GetItemType()
        {
            string artifactType = ArtifactType.ToString();

            if (ArtifactType.ToString().Contains("_"))
            {
                artifactType = artifactType.Replace("_", " ");
            }
            return artifactType;
        }

        public override string GetDescription()
        {
            sb.Length = 0;
            AddStat(LogisticsBonus, "Logistics");
            AddStat(LuckBonus, "Luck");
            AddStat(AttackBonus, "Attack");
            AddStat(DefenseBonus, "Defense");

            AddStat(LogisticsPercentBonus, "Logistics", isPercent: true);
            AddStat(LuckPercentBonus, "Luck", isPercent: true);
            AddStat(AttackPercentBonus, "Attack", isPercent: true);
            AddStat(DefensePercentBonus, "Defense", isPercent: true);

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

                if (isPercent)
                {
                    sb.Append(value * 100);
                    sb.Append("% ");
                }
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