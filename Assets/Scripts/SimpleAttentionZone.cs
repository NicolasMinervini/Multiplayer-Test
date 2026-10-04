using System.Collections.Generic;
using UnityEngine;

public class SimpleAttentionZone : MonoBehaviour
{
    public List<Entity> withinZone;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        withinZone = new List<Entity>();
    }

    public void OnTriggerEnter(Collider other)
    {
        //if (!IsOwner) return;

        Entity ent;

        if (other.TryGetComponent<Entity>(out ent) && withinZone.Contains(ent) == false)
        {
            withinZone.Add(ent);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        //if (!IsOwner) return;

        Entity ent;

        if (other.TryGetComponent<Entity>(out ent))
        {
            withinZone.Remove(ent);
        }
    }

    public Entity GetClosestEnemy(Entity self)
    {
        Entity foundEntity = null;
        float dist = -1;

        foreach(Entity ent in withinZone)
        {
            if (ent == null || ent == self || !ent.isAlive) continue;

            if(ent.team != self.team)
            {
                if (foundEntity == null)
                {
                    foundEntity = ent;
                    dist = Vector3.Distance(ent.transform.position, self.transform.position);
                }
                else if(Vector3.Distance(ent.transform.position, self.transform.position) < dist)
                {
                    foundEntity = ent;
                    dist = Vector3.Distance(ent.transform.position, self.transform.position);
                }
            }
        }

        return foundEntity;
    }

    public Entity GetFurthestEnemy(Entity self)
    {
        Entity foundEntity = null;
        float dist = -1;

        foreach (Entity ent in withinZone)
        {
            if (ent == null || ent == self || !ent.isAlive) continue;

            if (ent.team != self.team)
            {
                if (foundEntity == null)
                {
                    foundEntity = ent;
                    dist = Vector3.Distance(ent.transform.position, self.transform.position);
                }
                else if (Vector3.Distance(ent.transform.position, self.transform.position) > dist)
                {
                    foundEntity = ent;
                    dist = Vector3.Distance(ent.transform.position, self.transform.position);
                }
            }
        }

        return foundEntity;
    }

    public Entity GetRandomEntity(Entity self)
    {
        withinZone.RemoveAll(x => !x);

        Entity ent;
        int startPoint = Random.Range(0, withinZone.Count);
        int currentPoint = startPoint + 1;
        if (currentPoint >= withinZone.Count) currentPoint = 0;

        do
        {
            ent = withinZone[currentPoint];
            if (ent != null &&
                ent.team != self.team &&
                ent != self
                && ent.isAlive)
            {
                return ent;
            }

            currentPoint += 1;
            if (currentPoint >= withinZone.Count) currentPoint = 0;

        } while (currentPoint != startPoint);

        return null;
    }

    public int NearbyEnemyCount(Entity self)
    {
        withinZone.RemoveAll(x => !x);

        int count = 0;

        foreach(Entity ent in withinZone)
        {
            if (ent == null) continue;

            if(ent.team != self.team && ent != self || !ent.isAlive)
            {
                count += 1;
            }
        }

        return count;
    }
}
