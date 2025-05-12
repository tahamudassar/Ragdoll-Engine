using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject playerPrefab;  // Reference to the player prefab
    public Vector3 spawnPosition = new Vector3(0, 1.8f, 0);  // Where to spawn the player

    [Header("Camera Settings")]
    public Vector3 cameraOffset = new Vector3(0, 5, -7); // Desired offset for the camera
    private GameObject playerInstance;

    void Start()
    {
        // Instantiate the player at the specified position
        playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Player instantiated at position: " + spawnPosition);

        // Adjust the camera's parent position based on the player (if necessary)
        AdjustCameraPosition();
    }

    void AdjustCameraPosition()
    {
        Camera mainCam = Camera.main;  // Reference to the main camera
        if (mainCam != null)
        {
            // Get the CameraFollow script on the camera's parent (the empty GameObject)
            GameObject cameraParent = mainCam.transform.parent.gameObject;  // Get the parent of the camera

            // Adjust the camera's position relative to the player (based on offset)
            cameraParent.transform.position = playerInstance.transform.position + cameraOffset;
        }
        else
        {
            Debug.LogError("Main Camera not found!");
        }
    }
}
