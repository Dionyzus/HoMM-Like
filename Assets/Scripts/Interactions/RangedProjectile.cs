using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class RangedProjectile : MonoBehaviour
    {
        bool isTargetSet;
        [SerializeField]
        float projectileVelocity = 5f;

        float impactDistance = 0.2f;
        float combatCameraActivationDistance;

        UnitController targetUnit;
        public UnitController TargetUnit { get => targetUnit; set => targetUnit = value; }
        public bool IsTargetSet { get => isTargetSet; set => isTargetSet = value; }

        void Update()
        {
            if (isTargetSet)
            {
                Vector3 targetPosition = targetUnit.transform.position;
                targetPosition.y += 1;

                float distance = Vector3.Distance(transform.position, targetPosition);
                combatCameraActivationDistance = distance * 0.9f;

                if (targetUnit != null)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, projectileVelocity
                         * Time.deltaTime);

                    if (Vector3.Distance(transform.position, targetPosition) < combatCameraActivationDistance)
                    {
                        BattleManager.instance.ActivateCombatCamera(targetUnit.transform);
                    }
                    if (Vector3.Distance(transform.position, targetPosition) < impactDistance)
                    {
                        BattleManager.instance.currentUnit.ProjectileHitTarget = true;
                        BattleManager.instance.CurrentCombatEvent.OnDamageReceived();

                        Destroy(gameObject);
                    }
                }
                else
                {
                    Debug.Log("Target unit is null!");
                }
            }
            else
            {
                Debug.Log("Target is not set!");
            }
        }
    }
}