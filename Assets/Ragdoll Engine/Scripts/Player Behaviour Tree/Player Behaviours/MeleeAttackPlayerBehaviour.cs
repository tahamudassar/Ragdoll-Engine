using System;
using System.Collections.Generic;
using UnityEngine;

namespace RagdollEngine
{
    public class MeleeAttackPlayerBehaviour : PlayerBehaviour
    {
        [SerializeField] private Vector3 offsetVector;
        [SerializeField] private int attackDamage = 50;
        [SerializeField] private float cooldown;
        private float cooldownTimer = 0;
        public Action OnFire;


        public override bool Evaluate()
        {

            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }
            if (inputHandler.melee.pressed && cooldownTimer <= 0)
            {
                //Attack
                Attack();
                cooldownTimer = cooldown;
                return true;
            }
            return false;

        }


        private void Attack()
        {
            OnFire?.Invoke();
            //Overlapsphere with 2 radius in front of player,
            Vector3 attackPosition = modelTransform.position + modelTransform.forward * 2f + offsetVector;
            Collider[] hitColliders = Physics.OverlapSphere(attackPosition, 2f);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.transform.TryGetComponent(out IHittable hittable))
                {
                    hittable.DoHit(attackDamage);
                }
            }
            
        }




    }
}

