using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class SimpleUnit
    {
        UnitStatsReference unitStats;
        Node currentNode;
        UnitSide unitSide;
        int stepsCount;
        int unitId;

        public UnitStatsReference UnitStats { get => unitStats; set => unitStats = value; }
        public Node CurrentNode { get => currentNode; set => currentNode = value; }
        public UnitSide UnitSide { get => unitSide; set => unitSide = value; }

        int hitPoints;
        int damage;
        int attack;
        int defense;
        int initiative;
        public int HitPoints { get => hitPoints; set => hitPoints = value; }
        public int Damage { get => damage; set => damage = value; }
        public int Attack { get => attack; set => attack = value; }
        public int Defense { get => defense; set => defense = value; }
        public int Initiative { get => initiative; set => initiative = value; }
        public int StepsCount { get => stepsCount; set => stepsCount = value; }
        public int UnitId { get => unitId; set => unitId = value; }

        public SimpleUnit(UnitStatsReference unitStats, Node currentNode, UnitSide unitSide, int unitId)
        {
            this.unitStats = unitStats;
            InitializeUnitStats();

            this.currentNode = currentNode;
            this.unitSide = unitSide;
            this.unitId = unitId;
        }
        
        void InitializeUnitStats()
        {
            hitPoints = unitStats.HitPoints;
            damage = unitStats.Damage;
            attack = unitStats.Attack;
            defense = unitStats.Defense;
            initiative = unitStats.Initiative;
            stepsCount = unitStats.StepsCount;
        }
    }
}