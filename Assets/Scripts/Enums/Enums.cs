using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public enum GameState
    {
        BATTLE,
        WORLD
    }
    public enum UnitAttackType
    {
        MELEE,
        RANGED,
        MAGIC
    }
    public enum ScoreValue
    {
        STANDARD = 1,
        MEDIUM = 2,
        HIGH = 3,
        TOP = 5
    }
    public enum TargetPriority
    {
        INITITAL = 2,
        NEW = 5,
        FAVOURABLE = 10,
        TOP = 15
    }
    public enum UnitSide
    {
        MAX_UNIT,
        MIN_UNIT
    }
    public enum EvaluationScore
    {
        ATTACK_MOVE = 9,
        ROAMING_MOVE = 4
    }
    public enum EvaluationBoost
    {
        INITIATIVE = 2,
        STATS_SCORE = 5,
        UNITS_COUNT = 10
    }
    public enum ArtifactType
    {
        HELMET,
        CHEST_PLATE,
        GAUNTLETS,
        GREAVES,
        WEAPON,
        SHIELD,
        RING,
        AMULET,
        BELT,
        VAMBRACE
    }
}