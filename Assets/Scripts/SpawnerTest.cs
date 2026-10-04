using Unity.Netcode;
using UnityEngine;

public class SpawnerTest : NetworkBehaviour
{
    public GameObject spawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnObjectRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void SpawnObjectRpc()
    {
        GameObject sp = Instantiate(spawn, transform.position, Quaternion.identity);
        NetworkObject nb;
        if (sp.TryGetComponent<NetworkObject>(out nb))
        {
            nb.Spawn(true);
        }
    }
}
