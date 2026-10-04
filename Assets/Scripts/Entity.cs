using Unity.Netcode;
using UnityEngine;

public class Entity : NetworkBehaviour
{
    public float health = 10;
    public float currentHealth = 10;
    public bool isAlive = true;

    //TEAMS:
    //0 = player
    //1 = typical enemy
    public int team = 1;
    /*
    public int team
    {
        get
        {
            return networkTeam.Value;
        }
        set
        {
            networkTeam.Value = value;
        }
    }
    public NetworkVariable<int> networkTeam = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    */
    public GameObject deathEffect;
    public GameObject hitEffect;

    public virtual void Start()
    {
        currentHealth = health;
        if (NetworkManager.IsListening && !NetworkObject.IsSpawned) NetworkObject.Spawn(true);
    }

    [Rpc(SendTo.Owner)]
    public virtual void HurtRpc(float damage, Vector3 direction, Vector3 point, int otherteam, bool canBeParried = true, bool canBeBlocked = true)
    {
        //same team entities can not hurt each other
        if (team == otherteam) return;

        if(hitEffect != null)
        {
            //REMEMBER: the hit effect should face TOWARDS the direction the attack is moving in!

            Transform hiteffect = Instantiate(hitEffect, point, Quaternion.identity).transform;
            NetworkObject no;
            if(hiteffect.TryGetComponent<NetworkObject>(out no))
            {
                no.Spawn(true);
            }

            if(direction != Vector3.zero)
            {
                hiteffect.forward = direction.normalized;
            }
            else
            {
                //fallback if the attack didn't specify its direction of force
                hiteffect.forward = transform.position - point;
            }
        }

        currentHealth -= damage;
        if (currentHealth <= 0) DieRpc();
    }

    [Rpc(SendTo.Owner)]
    public virtual void HealRpc(float amount, Vector3 direction, int otherteam)
    {
        //entities of differing teams can not heal each other
        if (otherteam != team) return;

        currentHealth += amount;
        if (currentHealth > health) currentHealth = health;
    }

    [Rpc(SendTo.Owner)]
    public virtual void DieRpc()
    {
        if(deathEffect != null)
        {
            GameObject DE = Instantiate(deathEffect, transform.position, Quaternion.identity);
            NetworkObject no;
            if (DE.TryGetComponent<NetworkObject>(out no))
            {
                no.Spawn(true);
            }
        }

        //gameObject.SetActive(false);
        
        Destroy(gameObject);
    }

    //without this, entities pre-placed in the scene will still remain visible to clients even after they are destroyed
    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
        base.OnNetworkDespawn();
    }
}
