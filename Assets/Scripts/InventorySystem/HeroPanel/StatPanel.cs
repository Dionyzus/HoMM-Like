using UnityEngine;

namespace HOMM_BM
{
    public class StatPanel : MonoBehaviour
    {
        [SerializeField] StatDisplay[] statDisplays;
        [SerializeField] string[] statNames;

        private HeroStat[] stats;

        public StatDisplay[] StatDisplays { get => statDisplays; set => statDisplays = value; }
        public string[] StatNames { get => statNames; set => statNames = value; }

        private void OnValidate()
        {
            StatDisplays = GetComponentsInChildren<StatDisplay>();
            UpdateStatNames();
        }

        public void SetStats(params HeroStat[] charStats)
        {
            stats = charStats;

            if (stats.Length > StatDisplays.Length)
            {
                Debug.LogError("Not Enough Stat Displays!");
                return;
            }

            for (int i = 0; i < StatDisplays.Length; i++)
            {
                StatDisplays[i].gameObject.SetActive(i < stats.Length);

                if (i < stats.Length)
                {
                    StatDisplays[i].Stat = stats[i];
                }
            }
        }

        public void UpdateStatValues()
        {
            for (int i = 0; i < stats.Length; i++)
            {
                StatDisplays[i].UpdateStatValue();
            }
        }

        public void UpdateStatNames()
        {
            for (int i = 0; i < StatNames.Length; i++)
            {
                StatDisplays[i].Name = StatNames[i];
            }
        }
    }
}