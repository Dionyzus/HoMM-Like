using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

namespace HOMM_BM
{
    public static class FloatExtension
    {
        public static bool LessOrEqual(this float first, float second)
        {
            return first <= second;
        }
        public static bool Less(this float first, float second)
        {
            return first < second;
        }
        public static bool More(this float first, float second)
        {
            return first > second;
        }
    }
    public static class IntExtension
    {
        public static TargetPriority ConvertToTargetPriority(this int value)
        {
            if (value >= (int)TargetPriority.TOP)
            {
                return TargetPriority.TOP;
            }
            if (value >= (int)TargetPriority.FAVOURABLE)
            {
                return TargetPriority.FAVOURABLE;
            }
            if (value >= (int)TargetPriority.NEW)
            {
                return TargetPriority.NEW;
            }
            if (value >= (int)TargetPriority.INITITAL)
            {
                return TargetPriority.INITITAL;
            }
            return TargetPriority.INITITAL;
        }
        public static int CalculateDamage(this int hitPoints, int attackerStackSize, int defenderStackSize, int attack, int defense)
        {
            float attackPower = attack / defense;
            int stackDifference = attackerStackSize - defenderStackSize;

            if (stackDifference <= 0)
            {
                stackDifference = 1;
            }

            return hitPoints - (int)Math.Round(stackDifference * attackPower);
        }
    }
}