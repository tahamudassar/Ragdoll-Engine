using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class SplineObjectPlacer : MonoBehaviour
{
    [Header("Placement Settings")]
    public GameObject objectToPlace;
    public int numberOfObjects = 10;
    public bool alignRotation = true;

    [Header("Spline Options")]
    public bool useClosedSpline = false;

    private SplineContainer splineContainer;
    private Spline spline;

    void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null || splineContainer.Spline == null)
        {
            Debug.LogError("SplineContainer or Spline not found!");
            return;
        }

        spline = splineContainer.Spline;
        PlaceObjects();
    }

    void PlaceObjects()
    {
        // Validate input
        if (objectToPlace == null)
        {
            Debug.LogError("No object to place assigned!");
            return;
        }

        if (numberOfObjects < 1)
        {
            Debug.LogWarning("Number of objects must be at least 1");
            return;
        }

        // Calculate total spline length
        float splineLength = SplineUtility.CalculateLength(spline,transform.localToWorldMatrix);
        // Handle zero-length spline case
        if (Mathf.Approximately(splineLength, 0f))
        {
            Debug.LogWarning("Spline has zero length. Placing single object at start position.");
            PlaceSingleObject();
            return;
        }

        // Calculate placement parameters
        int placementCount = numberOfObjects;
        float stepSize = splineLength / (useClosedSpline ? numberOfObjects : numberOfObjects - 1);

        // Place objects along spline
        for (int i = 0; i < placementCount; i++)
        {
            float distance = i * stepSize;
            PlaceObjectAtDistance(distance, splineLength);
        }
    }

    void PlaceSingleObject()
    {
        Vector3 position = SplineUtility.EvaluatePosition(spline, 0f);
        Quaternion rotation = alignRotation
            ? Quaternion.LookRotation(SplineUtility.EvaluateTangent(spline, 0f))
            : Quaternion.identity;
        InstantiateObject(position, rotation);
    }

    void PlaceObjectAtDistance(float distance, float splineLength)
    {
        float t = SplineUtility.GetNormalizedInterpolation(spline, distance, PathIndexUnit.Distance);

        Vector3 localposition = SplineUtility.EvaluatePosition(spline, t);
        Vector3 position = splineContainer.transform.TransformPoint(localposition);
        Quaternion rotation = alignRotation
            ? Quaternion.LookRotation(SplineUtility.EvaluateTangent(spline, t))
            : Quaternion.identity;
        InstantiateObject(position, rotation);
    }

    void InstantiateObject(Vector3 position, Quaternion rotation)
    {
        Instantiate(objectToPlace, position, rotation, transform);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw instantiation points as spheres in edit mode
        splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null || splineContainer.Spline == null)
            return;

        spline = splineContainer.Spline;

        if (numberOfObjects < 1)
            return;

        float splineLength = SplineUtility.CalculateLength(spline, transform.localToWorldMatrix);
        if (Mathf.Approximately(splineLength, 0f))
        {
            // Draw single sphere at start
            Vector3 localPos = SplineUtility.EvaluatePosition(spline, 0f);
            Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(worldPos, 0.15f);
            return;
        }

        int placementCount = numberOfObjects;
        float stepSize = splineLength / (useClosedSpline ? numberOfObjects : numberOfObjects - 1);

        Gizmos.color = Color.cyan;
        for (int i = 0; i < placementCount; i++)
        {
            float distance = i * stepSize;
            float t = SplineUtility.GetNormalizedInterpolation(spline, distance, PathIndexUnit.Distance);
            Vector3 localPos = SplineUtility.EvaluatePosition(spline, t);
            Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);
            Gizmos.DrawSphere(worldPos, 1f);
        }
    }
#endif
}
