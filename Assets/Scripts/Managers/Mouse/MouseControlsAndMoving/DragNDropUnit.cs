using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HOMM_BM
{
    public class DragNDropUnit : MonoBehaviour, IPointerDownHandler, IEndDragHandler, IDragHandler
    {
        private Vector3 mOffset;
        private float mouseZCoord;

        UnitController unitController;
        Node initialNode;

        [HideInInspector]
        public List<Node> InvalidNodes = new List<Node>();

        public void OnPointerDown(PointerEventData eventData)
        {
            unitController = GetComponent<UnitController>();
            if (unitController != null)
                initialNode = unitController.CurrentNode;

            mouseZCoord = BattleManager.instance.MainCamera.WorldToScreenPoint(
                transform.position).z;

            mOffset = transform.position - GetMouseWorldPosition();
        }
        public void OnDrag(PointerEventData eventData)
        {
            transform.position = new Vector3(GetMouseWorldPosition().x + mOffset.x, transform.position.y, GetMouseWorldPosition().z + mOffset.z);
        }

        //Later on need to add walkable check
        public void OnEndDrag(PointerEventData eventData)
        {
            List<Node> tacticalNodes = BattleManager.instance.GetNormalizedTacticalNodes();
            Dictionary<UnitController, Node> unitsNodes = BattleManager.instance.GetUnitsNodes();

            Node normalizedNode = BattleManager.instance.CreateNormalizedNode(transform.position, unitController.GridIndex);

            if (normalizedNode == null || !tacticalNodes.Contains(normalizedNode) || !DropIsValid(unitsNodes, normalizedNode))
            {
                transform.position = initialNode.worldPosition;
                if (normalizedNode != null)
                    BattleManager.instance.InvalidNodes.Add(normalizedNode);
            }
            else
            {
                unitsNodes[unitController] = normalizedNode;
                transform.position = normalizedNode.worldPosition + new Vector3(0, 1, 0);
            }
        }

        bool DropIsValid(Dictionary<UnitController, Node> unitsNodes, Node normalizedNode)
        {
            if (unitsNodes.ContainsValue(normalizedNode))
            {
                return false;
            }
            foreach (Node subNode in normalizedNode.subNodes)
            {
                if (unitsNodes.ContainsValue(subNode))
                {
                    return false;
                }
            }
            foreach (Node node in unitsNodes.Values)
            {
                if (node.subNodes.Contains(normalizedNode))
                {
                    return false;
                }
            }
            return true;
        }
        Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = GameManager.instance.Mouse.position.ReadValue();
            mousePoint.z = mouseZCoord;

            return BattleManager.instance.MainCamera.ScreenToWorldPoint(mousePoint);
        }
    }
}