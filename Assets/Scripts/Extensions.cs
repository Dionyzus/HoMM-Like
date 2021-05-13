using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}