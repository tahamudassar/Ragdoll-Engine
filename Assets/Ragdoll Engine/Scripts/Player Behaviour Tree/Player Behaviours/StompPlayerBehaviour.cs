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

        [SerializeField] float stompAccelerationTime;

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
    }
}
