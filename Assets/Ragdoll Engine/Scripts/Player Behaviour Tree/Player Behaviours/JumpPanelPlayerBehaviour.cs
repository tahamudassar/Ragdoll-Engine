using UnityEngine;

namespace RagdollEngine
{
    public class JumpPanelPlayerBehaviour : PlayerBehaviour
    {
        [SerializeField] float jumpPanelDeadzone;

        public override bool Evaluate()
        {
            if (groundInformation.cast)
            {
                JumpPanelStageObject jumpPanelStageObject = groundInformation.hit.collider.GetComponentInParent<JumpPanelStageObject>();

                if (!jumpPanelStageObject || Vector3.Dot(RB.linearVelocity, jumpPanelStageObject.transform.forward) < jumpPanelDeadzone) return false;

                playerBehaviourTree.groundInformation.ground = true;

                additiveVelocity = -RB.linearVelocity
                    + (Quaternion.AngleAxis(jumpPanelStageObject.angle, -jumpPanelStageObject.transform.right) * jumpPanelStageObject.transform.forward * Mathf.Max(jumpPanelStageObject.speed, Vector3.Dot(RB.linearVelocity, jumpPanelStageObject.transform.forward)));

                if (wasActive) return true;

                jumpPanelStageObject.audioSource.Play();

                return true;
            }

            return false;
        }
    }
}
