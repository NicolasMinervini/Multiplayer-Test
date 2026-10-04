using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class RaptorEnemy : Entity
{
    public Entity target;
    public NavMeshAgent agent;
    public Animator anim;
    public SimpleAttentionZone attentionZone;

    public float attackDistance = 1f;
    public Vector2 timeBetweenAttacks = new Vector2(1.5f, 2f);
    float nextAttack = 0f;

    public float switchTargetAfterAttackChance = 0.25f;

    void Update()
    {
        if (!IsOwner) return;

        if(target != null && Time.time > nextAttack)
        {
            if(Vector3.Distance(transform.position, target.transform.position) <= attackDistance)
            {
                agent.isStopped = true;
                if(Time.time >= nextAttack)
                {
                    Attack();
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(target.transform.position);
            }
        }
        else
        {
            agent.isStopped = true;
            if(Time.time > nextAttack)
            {
                if(attentionZone.NearbyEnemyCount(this) > 0) target = attentionZone.GetClosestEnemy(this);
                else nextAttack = Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
            }
        }

        anim.SetBool("Moving", agent.velocity.magnitude >= 0.1f);
    }

    void Attack()
    {
        if (!IsOwner) return;

        agent.isStopped = true;
        anim.SetTrigger("Attack");

        if(target != null) agent.transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));

        nextAttack = Time.time + Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);

        //switches target if switching after every attack is enabled, or if the current target is no longer valid
        if((switchTargetAfterAttackChance > 0 && Random.Range(0f,1f) < switchTargetAfterAttackChance) || target == null || target.team == team || !target.isAlive)
        {
            target = attentionZone.GetRandomEntity(this);
        }
    }
}
