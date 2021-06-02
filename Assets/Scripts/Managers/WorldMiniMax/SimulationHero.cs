using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class SimulationHero
    {
        Node currentNode;
        UnitSide heroSide;
        int stepsCount;
        int heroId;

        public Node CurrentNode { get => currentNode; set => currentNode = value; }
        public UnitSide HeroSide { get => heroSide; set => heroSide = value; }
        public int StepsCount { get => stepsCount; set => stepsCount = value; }
        public int HeroId { get => heroId; set => heroId = value; }

        public SimulationHero(Node currentNode, UnitSide heroSide, int heroId, int stepsCount)
        {
            this.currentNode = currentNode;
            this.heroSide = heroSide;
            this.heroId = heroId;
            this.stepsCount = stepsCount;
        }
    }
}