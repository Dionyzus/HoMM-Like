using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public static class ResourcesManager
    {
        static ResourcesManagerAsset resourcesManager;

        public static ResourcesManagerAsset Instance
        {
            get
            {
                if (resourcesManager == null)
                {
                    resourcesManager = Resources.Load("ResourcesManager") as ResourcesManagerAsset;
                }

                return resourcesManager;
            }
        }
    }
}