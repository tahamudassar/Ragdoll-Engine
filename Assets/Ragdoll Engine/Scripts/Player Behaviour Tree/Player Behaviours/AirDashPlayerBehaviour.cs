using UnityEngine;

namespace RagdollEngine
{
    public class AirDashPlayerBehaviour : PlayerBehaviour
    {
        [SerializeField] private float dashLength = 10f;
        [SerializeField] private float dashSpeed=1.0f;
        private bool dashing=false;
        private bool canDash = true;
        private float currentLength=0f;
        private float speed=0f;
        private Vector3 dashStartPos=Vector3.zero;
        private Vector3 dashStartDir=Vector3.zero;
        void LateUpdate()
        {
            dashing = active
                || (dashing
                    && !groundInformation.ground
                    && RB.linearVelocity.magnitude >= 0);

            animator.SetBool("Dashing", dashing);
        }

        public override bool Evaluate()
        {
            if (!DashCheck()) return false;

            Vector3 goal = dashStartPos + (dashStartDir * (dashLength - currentLength));

            movePosition = goal;

            kinematic = true;

            modelTransform.rotation = Quaternion.LookRotation(dashStartDir, modelTransform.up);

            modelTransform.position = goal - (modelTransform.up * height);

            overrideModelTransform = true;

            currentLength = Mathf.Max(currentLength - (Mathf.Lerp(dashSpeed, speed, Vector3.Dot(RB.linearVelocity, dashStartDir) > 0 ? RB.linearVelocity.magnitude : 0) * Time.fixedDeltaTime), 0);

            if (currentLength <= 0)
                return false;

            return true;
        }

        bool DashCheck()
        {
            if (groundInformation.ground)
            {
                canDash = true;
                return false;
            }

            if (inputHandler.dash.pressed && canDash)
                {
                    if (wasActive) return true;
                    Debug.LogError("Dash");
                    dashStartPos = modelTransform.position;
                    dashStartDir = modelTransform.forward;
                    currentLength = dashLength;
                    speed = dashSpeed;
                    additiveVelocity = modelTransform.forward * dashSpeed;
                    animator.SetTrigger("Dash");
                    canDash = false;
                return true;
                }
            if (wasActive && currentLength > 0) return true;
            return false;
        }
    }

  
}
