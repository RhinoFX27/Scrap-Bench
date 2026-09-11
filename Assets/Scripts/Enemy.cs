using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    //How much health they have
    [SerializeField] private int health = 15;
    //How much they reduce damage by
    [SerializeField] private int armor;
    //Countdown to next attack - how many 'turns' between attacks
    [SerializeField] private int attackSpeed = 3;
    //how much damage they do when attacking
    [SerializeField] private int attackDamage = 4;

    private int attackCountdown;

    public int Health => health;
    public int Armor => armor;
    public int AttackCountdown => attackCountdown;
    public bool IsDead => health <= 0;

    private void Awake()
    {
        attackCountdown = attackSpeed;
    }

    public void TickCountdown()
    {
        attackCountdown--;
    }
    public void DelayAttack(int amount)
    {
        attackCountdown += amount;
        Debug.Log(name + "'s countdown increased to " + attackCountdown + ".");
    }

    public bool IsAttackReady()
    {
        return attackCountdown <= 0;
    }

    public void Attack(Player player)
    {
        player.TakeDamage(attackDamage);
        attackCountdown = attackSpeed;
        Debug.Log(name + " attacked for " + attackDamage + " damage.");
    }

    public void TakeDamage(int amount)
    {
        int absorbedDamage = Mathf.Min(armor, amount);

        armor -= absorbedDamage;
        amount -= absorbedDamage;
        health -= amount;

        health = Mathf.Max(health, 0);

        Debug.Log(
            name + " has " + health + " health and " + armor + " armor."
        );

        if(health <=0)
        {
            Debug.Log(name + " died!");
            gameObject.SetActive(false);
        }
    }
}
