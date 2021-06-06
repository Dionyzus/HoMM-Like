using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class HeroMoveInfo
    {
        SimulationHero hero;
        HeroMove heroMove;
        float distance;

        public HeroMoveInfo(SimulationHero hero, HeroMove heroMove, float distance)
        {
            this.hero = hero;
            this.heroMove = heroMove;
            this.distance = distance;
        }

        public SimulationHero Hero { get => hero; set => hero = value; }
        public HeroMove HeroMove { get => heroMove; set => heroMove = value; }
        public float Distance { get => distance; set => distance = value; }
    }
}