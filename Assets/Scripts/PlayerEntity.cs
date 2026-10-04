using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerEntity : Entity
{
    public PlayerAttack attack;
    public PlayerMovement move;
    public PlayerPotions potions;

    public TMP_Text healthText;

    public override void Start()
    {
        currentHealth = health;
        isAlive = true;

        if (!IsOwner) return;

        if(healthText == null)
        {
            GameObject ht = GameObject.Find("Health Text");
            if(ht != null && ht.TryGetComponent<TMP_Text>(out healthText))
            {
                UpdateHUDRpc();
            }
        }
    }

    [Rpc(SendTo.Owner)]
    public override void HurtRpc(float damage, Vector3 direction, Vector3 point, int otherteam, bool canBeParried = true, bool canBeBlocked = true)
    {
        //same team entities can not hurt each other
        if (team == otherteam) return;

        if (canBeParried && attack.TryParry()) return;

        if (canBeBlocked && attack.TryBlock()) return;

        if (hitEffect != null)
        {
            //REMEMBER: the hit effect should face TOWARDS the direction the attack is moving in!

            SpawnHurtParticleRpc(point, direction);
        }

        currentHealth -= damage;
        if (currentHealth <= 0) DieRpc();

        UpdateHUDRpc();
    }

    [Rpc(SendTo.Owner)]
    public override void HealRpc(float amount, Vector3 direction, int otherteam)
    {
        //entities of DIFFERING teams can not heal each other
        if (otherteam != team) return;

        currentHealth += amount;
        if (currentHealth > health) currentHealth = health;

        UpdateHUDRpc();
    }

    [Rpc(SendTo.Owner)]
    public override void DieRpc()
    {
        if (deathEffect != null)
        {
            SpawnDeathParticleRpc();
        }

        potions.canDrink = false;
        move.canMove = false;
        attack.canAttack = false;
    }

    [Rpc(SendTo.Owner)]
    public void UpdateHUDRpc()
    {
        if (!IsOwner) return;

        if(healthText != null)
        {
            healthText.text = "HP: " + currentHealth;
        }
    }


    [Rpc(SendTo.Server)]
    public void SpawnHurtParticleRpc(Vector3 point, Vector3 direction)
    {
        Transform hiteffect = Instantiate(hitEffect, point, Quaternion.identity).transform;
        NetworkObject no;
        if (hiteffect.TryGetComponent<NetworkObject>(out no))
        {
            no.Spawn(true);
        }

        if (direction != Vector3.zero)
        {
            hiteffect.forward = direction.normalized;
        }
        else
        {
            //fallback if the attack didn't specify its direction of force
            hiteffect.forward = transform.position - point;
        }
    }
    [Rpc(SendTo.Server)]
    public void SpawnDeathParticleRpc()
    {
        GameObject DE = Instantiate(deathEffect, transform.position, Quaternion.identity);
        NetworkObject no;
        if (DE.TryGetComponent<NetworkObject>(out no))
        {
            no.Spawn(true);
        }
    }
}
