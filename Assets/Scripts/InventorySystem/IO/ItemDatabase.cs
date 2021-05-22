using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HOMM_BM
{
    [CreateAssetMenu]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] Item[] items;

        public Item[] Items { get => items; set => items = value; }

        public Item GetItemReference(string itemID)
        {
            foreach (Item item in Items)
            {
                if (item.ID == itemID)
                {
                    return item;
                }
            }
            return null;
        }

        public Item GetItemCopy(string itemID)
        {
            Item item = GetItemReference(itemID);
            return item != null ? item.GetCopy() : null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            LoadItems();
        }

        private void OnEnable()
        {
            EditorApplication.projectChanged -= LoadItems;
            EditorApplication.projectChanged += LoadItems;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= LoadItems;
        }

        private void LoadItems()
        {
            Items = FindAssetsByType<HOMM_BM.Item>("Assets/Data/Items");
        }

        public static T[] FindAssetsByType<T>(params string[] folders) where T : Object
        {
            string type = typeof(T).Name;

            string[] guids;
            if (folders == null || folders.Length == 0)
            {
                guids = AssetDatabase.FindAssets("t:" + type);
            }
            else
            {
                guids = AssetDatabase.FindAssets("t:" + type, folders);
            }

            T[] assets = new T[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }
            return assets;
        }
#endif
    }
}