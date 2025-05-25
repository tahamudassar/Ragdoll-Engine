using System;
using System.Collections.Generic;
using UnityEngine;

namespace RagdollEngine
{
    public class MeleeAttackPlayerBehaviour : PlayerBehaviour
    {
        [SerializeField] private Vector3 offsetVector;
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
            // Here you can implement the logic for melee attack, such as dealing damage to enemies
            // For example, you might want to check for nearby enemies and apply damage to them.
            Debug.Log("Melee attack executed at position: " + (character.transform.position + offsetVector));
        }




    }
}

