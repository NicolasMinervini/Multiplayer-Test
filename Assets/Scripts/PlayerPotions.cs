using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerPotions : NetworkBehaviour
{
    public PlayerAttack atk;
    public TMP_Text potionText;

    public float potionAnimationLength = 1.1f;
    public float healAmount = 10f;
    public int potionCount = 5;
    public int currentPotions = 5;

    public float potionEffectDelay = 0;

    public bool canDrink = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPotions = potionCount;
        canDrink = true;

        if (!IsOwner) return;

        if (potionText == null)
        {
            GameObject ht = GameObject.Find("Potion Text");
            if (ht != null && ht.TryGetComponent<TMP_Text>(out potionText))
            {
                UpdateHUD();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        //healing input

        if (Input.GetKeyDown(KeyCode.R) && Time.time >= atk.nextAttack && canDrink)
        {
            if (currentPotions > 0)
            {
                UsePotion();
            }
            else
            {
                //no potions left
                NoPotion();
            }
        }
        else if(atk.healing == false && Time.time < atk.nextAttack)
        {
            atk.anim.ResetTrigger("Heal");
            atk.anim.ResetTrigger("No Heal");
        }
    }

    void UsePotion()
    {
        if (!IsOwner) return;

        if (potionEffectDelay > 0)
        {
            Invoke("PotionEffect", potionEffectDelay);
        }
        else
        {
            PotionEffect();
        }

        atk.anim.SetTrigger("Heal");

        atk.nextAttack = Time.time + potionAnimationLength;
        atk.healing = true;
    }

    void NoPotion()
    {
        if (!IsOwner) return;

        atk.anim.SetTrigger("No Heal");

        atk.nextAttack = Time.time + potionAnimationLength;
        atk.healing = true;
    }

    void PotionEffect()
    {
        if (!IsOwner) return;

        atk.ent.HealRpc(healAmount, Vector3.zero, atk.ent.team);

        currentPotions -= 1;

        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (!IsOwner) return;

        if(potionText != null)
        {
            potionText.text = currentPotions.ToString();
        }
    }
}
