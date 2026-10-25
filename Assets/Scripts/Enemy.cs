using UnityEngine;
using TMPro;
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
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private TMP_Text enemyHPText;
    [SerializeField] private TMP_Text enemyCounterText;

    private int attackCountdown;

    public int Health => health;
    public int Armor => armor;
    public int AttackCountdown => attackCountdown;
    public bool IsDead => health <= 0;

    private void Awake()
    {
        attackCountdown = attackSpeed;
        enemyCounterText.text = attackCountdown.ToString();
    }

    public void TickCountdown()
    {
        attackCountdown--;
    }
    public void DelayAttack(int amount)
    {
        attackCountdown += amount;
        Debug.Log(name + "'s countdown increased to " + attackCountdown + ".");
        EffectTextQueue.Enqueue(name + "'s countdown increased to " + attackCountdown + "!");
        enemyCounterText.text = attackCountdown.ToString();
    }

    public bool IsAttackReady()
    {
        return attackCountdown <= 0;
    }

    public void Attack(PlayerManager player)
    {
        player.TakeDamage(attackDamage);
        attackCountdown = attackSpeed;
        Debug.Log(name + " attacked for " + attackDamage + " damage.");
        EffectTextQueue.Enqueue(name + " attacked for " + attackDamage + " damage!");
        enemyCounterText.text = attackCountdown.ToString();
    }

    public void TakeDamage(int amount)
    {
        int absorbedDamage = Mathf.Min(armor, amount);

        armor -= absorbedDamage;
        amount -= absorbedDamage;
        health -= amount;
        enemyHPText.text = health.ToString();

        health = Mathf.Max(health, 0);
        Debug.Log(
            name + " has " + health + " health and " + armor + " armor."
        );
        EffectTextQueue.Enqueue(name + " has " + health + " health and " + armor + " armor!");

        if(health <=0)
        {
            Debug.Log(name + " died!");
            EffectTextQueue.Enqueue(name + " died!");
            gameObject.SetActive(false);
        }
    }
}
