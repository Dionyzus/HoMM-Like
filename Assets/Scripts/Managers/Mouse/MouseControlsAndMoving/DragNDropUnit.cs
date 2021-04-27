using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HOMM_BM
{
    public class DragNDropUnit : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Vector3 mOffset;
        private float mouseZCoord;

        UnitController unitController;
        Node initialNode;

        public void OnPointerDown(PointerEventData eventData)
        {
            unitController = GetComponent<UnitController>();
            if (unitController != null)
                initialNode = unitController.CurrentNode;

            mouseZCoord = BattleManager.instance.MainCamera.WorldToScreenPoint(
                transform.position).z;

            mOffset = transform.position - GetMouseWorldPosition();
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            Cursor.visible = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = new Vector3(GetMouseWorldPosition().x + mOffset.x, transform.position.y, GetMouseWorldPosition().z + mOffset.z);
        }

        //Later on need to add walkable check
        public void OnEndDrag(PointerEventData eventData)
        {
            Cursor.visible = true;

            List<Node> tacticalNodes = BattleManager.instance.GetNormalizedTacticalNodes();

            Vector3 normalizedVector = new Vector3(Mathf.Round(transform.position.x), 1, Mathf.Round(transform.position.z));
            Node node = GridManager.instance.GetNode(normalizedVector, unitController.gridIndex);

            if (node == null || !tacticalNodes.Contains(node))
            {
                transform.position = initialNode.worldPosition;
            }
            else
            {
                transform.position = node.worldPosition;
            }
        }
        Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = GameManager.instance.Mouse.position.ReadValue();
            mousePoint.z = mouseZCoord;

            return BattleManager.instance.MainCamera.ScreenToWorldPoint(mousePoint);
        }
    }
}