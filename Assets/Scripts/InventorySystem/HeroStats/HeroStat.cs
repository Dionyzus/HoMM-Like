using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HOMM_BM
{
	[Serializable]
	public class HeroStat
	{
		public float BaseValue;

		protected bool isDirty = true;
		protected float lastBaseValue;

		protected float value;
		public virtual float Value
		{
			get
			{
				if (isDirty || lastBaseValue != BaseValue)
				{
					lastBaseValue = BaseValue;
					value = CalculateFinalValue();
					isDirty = false;
				}
				return value;
			}
		}

		protected readonly List<StatModifier> statModifiers;
		public readonly ReadOnlyCollection<StatModifier> StatModifiers;

		public HeroStat()
		{
			statModifiers = new List<StatModifier>();
			StatModifiers = statModifiers.AsReadOnly();
		}

		public HeroStat(float baseValue) : this()
		{
			BaseValue = baseValue;
		}

		public virtual void AddModifier(StatModifier mod)
		{
			isDirty = true;
			statModifiers.Add(mod);
			statModifiers.Sort(CompareModifierOrder);
		}

		public virtual bool RemoveModifier(StatModifier mod)
		{
			if (statModifiers.Remove(mod))
			{
				isDirty = true;
				return true;
			}
			return false;
		}

		public virtual bool RemoveAllModifiersFromSource(object source)
		{
			bool didRemove = false;

			for (int i = statModifiers.Count - 1; i >= 0; i--)
			{
				if (statModifiers[i].Source == source)
				{
					isDirty = true;
					didRemove = true;
					statModifiers.RemoveAt(i);
				}
			}
			return didRemove;
		}

		protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
		{
			if (a.Order < b.Order)
				return -1;
			else if (a.Order > b.Order)
				return 1;
			return 0; //if (a.Order == b.Order)
		}
		
		protected virtual float CalculateFinalValue()
		{
			float finalValue = BaseValue;
			float sumPercentAdd = 0;

			for (int i = 0; i < statModifiers.Count; i++)
			{
				StatModifier mod = statModifiers[i];

				if (mod.Type == StatModType.Flat)
				{
					finalValue += mod.Value;
				}
				else if (mod.Type == StatModType.PercentAdd)
				{
					sumPercentAdd += mod.Value;

					if (i + 1 >= statModifiers.Count || statModifiers[i + 1].Type != StatModType.PercentAdd)
					{
						finalValue *= 1 + sumPercentAdd;
						sumPercentAdd = 0;
					}
				}
				else if (mod.Type == StatModType.PercentMult)
				{
					finalValue *= 1 + mod.Value;
				}
			}

			return (float)Math.Round(finalValue, 4);
		}
	}
}
