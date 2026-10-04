using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerAttack : NetworkBehaviour
{
    public PlayerEntity ent;
    public Animator anim;

    public bool playerOwnsProjectiles = true;

    public Transform fireballSource;
    public GameObject fireball;
    public float fireballCooldown = 2f;
    [HideInInspector]
    public float nextFireball = 0f;

    public bool canAttack = true;

    public bool armed = true;
    public bool healing = false;
    public bool blocking = false;

    public float lastTriedAttack = -99f;
    //Span of time where the player can try to attack again before current attack animation ends
    public float saveAttackInputLength = 0.25f;

    public float lightAttackLength = 0.33f;
    public float heavyAttackLength = 1f;

    [HideInInspector]
    public float nextAttack = 0f;

    float lastNotGuarding = 0f;
    public float parryWindow = 0.15f;
    //if true, successful parries extend parrying window
    public bool extendingParries = false;
    public GameObject parryEffect;
    public Transform parryEffectSource;

    void Start()
    {
        if (!IsOwner) return;

        if (playerOwnsProjectiles)
        {
            Debug.LogError("TODO: player owning projectiles causes error where the projectile's team isn't updated right, causing the player to get hurt by their own projectiles.");
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if(ent != null && Input.GetKeyDown(KeyCode.T))
        {
            ent.team += 1;
            if(ent.team == 2) { ent.team = -1; }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            armed = !armed;
        }

        blocking = Input.GetMouseButton(1) && armed && canAttack;
        if (!blocking) lastNotGuarding = Time.time;

        //sets animator data

        if (healing && Time.time >= nextAttack) healing = false;
        anim.SetBool("Armed", armed && !healing);
        anim.SetBool("Blocking", blocking);

        //attack stuff

        if(canAttack && Time.time >= nextAttack)
        {
            //checks if the player tried to continue a combo. If not, waits for the player to try attacking again
            if(Time.time <= lastTriedAttack + saveAttackInputLength)
            {
                //continue combo
                if (Input.GetKey(KeyCode.LeftShift)) Attack(2);
                else Attack(1);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                //attack
                if (Input.GetKey(KeyCode.LeftShift)) Attack(2);
                else Attack();
            }
        }
        else if(canAttack && Input.GetMouseButtonDown(0))
        {
            //store attempts to attack while a previous attack is still acting

            lastTriedAttack = Time.time;
        }

        //fireball test

        if(Time.time >= nextFireball && Input.GetKeyDown(KeyCode.F) && ent != null && fireballSource != null)
        {
            SpawnProjectileRpc(fireballSource.position, fireballSource.rotation, ent.team, OwnerClientId, playerOwnsProjectiles);
            nextFireball = Time.time + fireballCooldown;
        }
    }


    public void Attack(int type = 0)
    {
        if (!armed || !canAttack) return;

        switch (type)
        {
            case 0:
                anim.Play("Idle Armed");
                //anim.Play("Attack", 0, 0f);
                //anim.Play("Attack");
                anim.SetTrigger("Light Attack");
                nextAttack = Time.time + lightAttackLength;
                break;
            case 1:
                anim.Play("Idle Armed");
                //anim.Play("Attack 2", 0, 0f);
                //anim.Play("Attack 2");
                anim.SetTrigger("Combo Attack");
                nextAttack = Time.time + lightAttackLength;
                break;
            default:
                anim.Play("Idle Armed");
                //anim.Play("Attack 3", 0, 0f);
                //anim.Play("Attack 3");
                anim.SetTrigger("Heavy Attack");
                nextAttack = Time.time + heavyAttackLength;
                break;
        }
    }

    public bool TryParry()
    {
        if(!blocking || Time.time > lastNotGuarding + parryWindow)
        {
            return false;
        }
        else
        {
            //Commented out to allow parrying of multiple attacks with one input. Because that is awesome.
            //lastNotGuarding = -99f;

            Debug.Log("Parried an attack!");

            SpawnParryEffect();

            if (extendingParries)
            {
                lastNotGuarding = Time.time - 0.1f;
            }

            return true;
        }
    }
    public bool TryBlock()
    {
        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);

        if (!blocking)
        {
            return false;
        }
        else if(animState.IsName("Guard") || animState.IsName("Block Attack"))
        {
            Debug.Log("Blocked an attack!");

            anim.Play("Guard");
            anim.SetTrigger("Deflect");

            return true;
        }
        else
        {
            return false;
        }
    }

    //Immediately spawn a local effect to ensure it shows at the right time for the player parrying
    public void SpawnParryEffect()
    {
        GameObject parryeffect = Instantiate(parryEffect, parryEffectSource);
        SpawnParryEffectRpc();
    }

    //Spawn the effect for everyone else to see. Will be delayed
    [Rpc(SendTo.NotOwner)]
    public void SpawnParryEffectRpc()
    {
        GameObject parryeffect = Instantiate(parryEffect, parryEffectSource);
    }

    //Creates an object and spawns it on the network
    [Rpc(SendTo.Server)]
    void SpawnProjectileRpc(Vector3 position, Quaternion rotation, int team, ulong newOwner, bool setNewOwner = false)
    {
        if (fireball == null || fireballSource == null) { return; }

        //instantiate projectile
        GameObject proj = Instantiate(fireball, position, rotation);

        Projectile p;
        if (proj.TryGetComponent<Projectile>(out p))
        {
            p.dead = true;
        }

        //spawn object on network
        NetworkObject no;
        if (proj.TryGetComponent<NetworkObject>(out no))
        {
            no.Spawn(true);
        }

        if (p != null)
        {
            p.team = team;
            p.dead = false;
        }

        //set projectile's ownership
        if (setNewOwner && no != null && no.OwnerClientId != newOwner)
        {
            no.ChangeOwnership(newOwner);
        }
    }
}
