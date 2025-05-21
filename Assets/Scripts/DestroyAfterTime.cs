using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float destroyTime = 1f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
