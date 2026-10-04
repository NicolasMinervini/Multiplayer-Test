using Unity.Netcode;
using UnityEngine;

public class DestroyAfterTime : NetworkBehaviour
{
    public float lifetime = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("Die", lifetime);
    }

    void Die()
    {
        if (IsOwner)
        {
            Destroy(gameObject);
        }
    }
}
