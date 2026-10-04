using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public Rigidbody rb;
    public float forwardVelocity = 0f;

    public List<Entity> hitEntities = new List<Entity>();

    public float damage = 1f;
    public float lifetime = 0.5f;

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

    public bool destroyAfterHit = false;
    public bool canBeParried = true;

    Vector3 previousPosition;

    public bool giveObjectDirection = true;
    public bool alwaysGetNextPosition = false;

    public bool breakOnGround = false;
    public bool breakOnHit = false;

    float dieTime = 9999999;
    public bool dead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!IsOwner) return;

        if(rb != null && forwardVelocity != 0)
        {
            rb.linearVelocity = transform.forward * forwardVelocity;
        }

        dieTime = Time.time + lifetime;
    }

    private void Update()
    {
        if (!IsOwner || dead) return;

        if (alwaysGetNextPosition || previousPosition != transform.position)
        {
            previousPosition = transform.position;
        }

        if(Time.time >= dieTime)
        {
            killProjectile();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner || dead) return;

        Entity hit;
        if (other.TryGetComponent<Entity>(out hit)
            && !hitEntities.Contains(hit)
            && team != hit.team)
        {
            //if (!hit.IsOwner) return;

            //determine direction of attack's movement
            Vector3 posDir = Vector3.zero;
            if (giveObjectDirection)
            {
                posDir = (transform.position - previousPosition).normalized;
            }

            hit.HurtRpc(damage, posDir, other.ClosestPoint(transform.position), team, canBeParried);
            hitEntities.Add(hit);
            if (breakOnHit)
            {
                //Destroy(gameObject);
                Debug.Log("Projectile " + gameObject.name + " with team " + team + " hit gameobject " + hit.name + " with team " + hit.team);
                killProjectile();
            }
        }

        if(breakOnGround && other.gameObject.layer == 0)
        {
            //Destroy(gameObject);
            killProjectile();
        }
    }

    void killProjectile()
    {
        dead = true;
        DestructRpc();
    }
    
    [Rpc(SendTo.Server)]
    void DestructRpc()
    {
        Destroy(gameObject);
    }
}
