using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class SimpleUnit
    {
        UnitStats unitStats;
        Node currentNode;
        int layer;
        int stepsCount;

        public UnitStats UnitStats { get => unitStats; set => unitStats = value; }
        public Node CurrentNode { get => currentNode; set => currentNode = value; }
        public int Layer { get => layer; set => layer = value; }

        int hitPoints;
        int damage;
        int attack;
        int defense;
        float initiative;
        public int HitPoints { get => hitPoints; set => hitPoints = value; }
        public int Damage { get => damage; set => damage = value; }
        public int Attack { get => attack; set => attack = value; }
        public int Defense { get => defense; set => defense = value; }
        public float Initiative { get => initiative; set => initiative = value; }
        public int StepsCount { get => stepsCount; set => stepsCount = value; }

        public SimpleUnit(UnitStats unitStats, Node currentNode, int layer)
        {
            this.unitStats = unitStats;
            InitializeUnitStats();

            this.currentNode = currentNode;
            this.layer = layer;
        }
        
        void InitializeUnitStats()
        {
            hitPoints = unitStats.hitPoints;
            damage = unitStats.damage;
            attack = unitStats.attack;
            defense = unitStats.defense;
            initiative = unitStats.initiative;
            stepsCount = unitStats.stepsCount;
        }
    }
}