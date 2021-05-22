using UnityEngine;

namespace HOMM_BM
{
    public class InventoryInput : MonoBehaviour
    {
        [SerializeField] GameObject inventoryPanelGameObject;
        [SerializeField] KeyCode[] toggleInventoryKeys;
        [SerializeField] bool showAndHideMouse = true;

        public GameObject InventoryPanelGameObject { get => inventoryPanelGameObject; set => inventoryPanelGameObject = value; }
        public KeyCode[] ToggleInventoryKeys { get => toggleInventoryKeys; set => toggleInventoryKeys = value; }
        public bool ShowAndHideMouse { get => showAndHideMouse; set => showAndHideMouse = value; }

        private void Start()
        {
            InventoryPanelGameObject.SetActive(false);
        }
        void Update()
        {
            ToggleInventory();
        }

        private void ToggleInventory()
        {
            for (int i = 0; i < ToggleInventoryKeys.Length; i++)
            {
                if (GameManager.instance.Keyboard.iKey.wasPressedThisFrame)
                {
                    InventoryPanelGameObject.SetActive(!InventoryPanelGameObject.activeSelf);

                    if (InventoryPanelGameObject.activeSelf)
                    {
                        InventoryPanelGameObject.SetActive(true);
                    }
                    else
                    {
                        StatTooltip[] statTooltips = FindObjectsOfType<StatTooltip>();
                        foreach (StatTooltip statTooltip in statTooltips)
                        {
                            if (statTooltip.gameObject.activeSelf)
                            {
                                statTooltip.HideTooltip();
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }

        public void ShowMouseCursor()
        {
            if (ShowAndHideMouse)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        public void HideMouseCursor()
        {
            if (ShowAndHideMouse)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}