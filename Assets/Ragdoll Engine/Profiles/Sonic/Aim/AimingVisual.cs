using RagdollEngine;
using UnityEngine;

public class AimingVisual : MonoBehaviour
{
    [SerializeField] private GameObject aimTarget;
    [SerializeField] private RangedAttackPlayerBehaviour aimBehaviour;
    [SerializeField] private LineRenderer lineRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  
    private void LateUpdate()
    {
        if (aimBehaviour != null)
        {
            // Get the points from the aim behaviour
            var points = aimBehaviour.getPoints();
            // Set the position of the aim target to the last point
            if (points != null)
            {
                if (points.Count > 0)
                {
                    if (points.Count != aimBehaviour.GetMaxPoints())
                    {
                        aimTarget.SetActive(true);
                        aimTarget.transform.position = points[points.Count - 1];
                    }
                    else
                    {
                        aimTarget.SetActive(false);
                    }
                        lineRenderer.positionCount = points.Count;
                    for (int i = 0; i < points.Count; i++)
                    {
                        lineRenderer.SetPosition(i, points[i]);
                    }
                }

            }
        }
    }
}
