using Unity.Netcode;
using UnityEngine;

public class FireSpitter : Entity
{
    public GameObject fireProjectile;
    public Transform projectileSource;

    public float timeBetweenProjectiles = 1f;
    float nextProjectile = 1f;

    void Update()
    {
        if (!IsOwner) return;

        if(Time.time > nextProjectile)
        {
            SpawnProjectileRpc(projectileSource.position, projectileSource.rotation, team, OwnerClientId, false);
            nextProjectile = Time.time + timeBetweenProjectiles;
        }
    }

    //Creates an object and spawns it on the network
    [Rpc(SendTo.Server)]
    void SpawnProjectileRpc(Vector3 position, Quaternion rotation, int team, ulong newOwner, bool setNewOwner = false)
    {
        if (fireProjectile == null) { return; }

        //instantiate projectile
        GameObject proj = Instantiate(fireProjectile, position, rotation);

        //spawn object on network
        NetworkObject no;
        if (proj.TryGetComponent<NetworkObject>(out no))
        {
            no.Spawn(true);
        }

        //set projectile's tean
        Projectile p;
        if (proj.TryGetComponent<Projectile>(out p))
        {
            p.team = team;
        }

        //set projectile's ownership
        if (setNewOwner && no != null && no.OwnerClientId != newOwner)
        {
            no.ChangeOwnership(newOwner);
        }
    }
}
