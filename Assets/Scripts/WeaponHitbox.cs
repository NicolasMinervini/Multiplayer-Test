using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    public Entity self;
    public List<Entity> hitEntities = new List<Entity>();
    public float damage = 1f;

    public float lifetime = 0.5f;
    float disableTime = 0;

    Vector3 previousPosition;

    //if true, this hitbox will give the direction it is moving as the attack direction to hurt functions. Otherwise it gives Vectro3.zero.
    public bool giveObjectDirection = true;
    public bool alwaysGetNextPosition = false;

    private void OnEnable()
    {
        if (!self.IsOwner) return;

        hitEntities.Clear();

        disableTime = Time.time + lifetime;

        if(alwaysGetNextPosition || previousPosition != transform.position)
        {
            previousPosition = transform.position;
        }
    }

    private void Update()
    {
        if (!self.IsOwner) return;

        previousPosition = transform.position;

        if (Time.time >= disableTime)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!self.IsOwner) return;

        Entity hit;
        if (other.TryGetComponent<Entity>(out hit)
            && !hitEntities.Contains(hit)
            && (self == null || self.team != hit.team))
        {
            //if (!hit.IsOwner) return;

            //determine direction of attack's movement
            Vector3 posDir = Vector3.zero;
            if (giveObjectDirection)
            {
                posDir = (transform.position - previousPosition).normalized;
            }

            //Debug.Log(previousPosition + " ----- " + transform.position + "---------- " + posDir);

            hit.HurtRpc(damage, posDir, other.ClosestPoint(transform.position), self.team);
            hitEntities.Add(hit);
        }
    }

    public void ForceEnd()
    {
        disableTime = Time.time - 1f;
        gameObject.SetActive(false);
    }
}
