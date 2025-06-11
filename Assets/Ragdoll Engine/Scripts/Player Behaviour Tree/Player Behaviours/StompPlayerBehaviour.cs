using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace RagdollEngine
{
    public class StompPlayerBehaviour : PlayerBehaviour
    {
        [SerializeField] AudioSource landAudioSource;

        [SerializeField] float minStompForce;

        [SerializeField] float maxStompForce;
        [SerializeField] float bounceMult = 10f;
        [SerializeField] int bounceDamage = 50;
        [SerializeField] float stompAccelerationTime;
        private Vector3 bouncePos;

        float stompAccelerationTimer;
        bool bounced = false;
        void LateUpdate()
        {
            animator.SetBool("Stomping", active);

            if (wasActive && groundInformation.ground)
                landAudioSource.Play();
        }

        public override bool Evaluate()
        {
            bool stomping = (inputHandler.stomp.pressed || wasActive) && !groundInformation.ground;
            bool ground = groundInformation.ground;

            if (stomping)
            {
                if (wasActive)
                    stompAccelerationTimer -= Time.fixedDeltaTime;
                else
                {
                    stompAccelerationTimer = stompAccelerationTime;

                    animator.SetTrigger("Stomp");
                }
                bounced = false;
            }
            else if (ground && !bounced && wasActive)
            {
                additiveVelocity = -RB.linearVelocity + playerTransform.up * bounceMult;
                accelerationVector = Vector3.zero;
                stickToGround = false;
                bounced = true;
                BounceAttack();
            }
            return stomping;
        }

        public override void Execute()
        {
            if (!bounced)
            {
                additiveVelocity = -RB.linearVelocity
                    + (-Vector3.up * Mathf.Lerp(minStompForce, maxStompForce, 1 - Mathf.Pow(10, -(1 - (stompAccelerationTimer / stompAccelerationTime)))));
            }
        }
        private void BounceAttack()
        {
            //Check if there are hittables within a sphere of 1m and damage them
            Collider[] hitColliders = Physics.OverlapSphere(playerTransform.position, 2f);
            foreach (Collider hitCollider in hitColliders)
            {

                if (hitCollider.transform.TryGetComponent(out IHittable hittable))
                {
                    print($"Hit {hittable} with bounce damage of {bounceDamage} at position {playerTransform.position}");
                    hittable.DoHit(bounceDamage);
                }
            }
            bouncePos = playerTransform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(bouncePos, 2f);
        }
    }
}
