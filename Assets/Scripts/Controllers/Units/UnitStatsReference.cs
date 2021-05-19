using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class UnitStatsReference
    {
        int hitPoints;
        int damage;
        int attack;
        int defense;
        int initiative;
        int stepsCount;
        public int HitPoints { get => hitPoints; set => hitPoints = value; }
        public int Damage { get => damage; set => damage = value; }
        public int Attack { get => attack; set => attack = value; }
        public int Defense { get => defense; set => defense = value; }
        public int Initiative { get => initiative; set => initiative = value; }
        public int StepsCount { get => stepsCount; set => stepsCount = value; }

        public UnitStatsReference(int hitPoints, int damage, int attack, int defense, int initiative, int stepsCount)
        {
            this.hitPoints = hitPoints;
            this.damage = damage;
            this.attack = attack;
            this.defense = defense;
            this.initiative = initiative;
            this.stepsCount = stepsCount;
        }
    }
}