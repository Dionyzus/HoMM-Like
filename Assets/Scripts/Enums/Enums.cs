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
    public enum StatModType
    {
        FLAT = 100,
        PERCENT_ADD = 200,
        PERCENT_MULT = 300,
    }
    public enum UnitType
    {
        MUTANT,
        NIGHTSHADE,
        ZOMBIE,
        VAMPIRE
    }
    public enum StackSplit
    {
        MINIMAL = 1,
        REGULAR = 2,
        MAXIMAL = 3
    }
    public enum StackDescription
    {
        NORMAL = 15,
        LARGE = 30,
        HORDE = 50
    }
    public enum InteractionType
    {
        ENEMY_UNIT,
        COLLECTABLE,
        COMBAT,
        FRIENDLY
    }
}