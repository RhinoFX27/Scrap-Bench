using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{

    [Header("Player Stats")]
    //When health reaches 0 you lose the fight
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int currentHealth = 30;
    //Amount of damage reduction the player gets
    [SerializeField] private int armor = 0;
    [Header("Weapon Stats")]
    //Damage without a crit 
    [SerializeField] private int damage = 5;
    //Amount of ''turns'' between shots
    [SerializeField] private int fireRate = 3;
    //How likely you are to hit a shot
    [SerializeField, Range(0, 100)] private int accuracy = 75;
    //Damage with a crit
    [SerializeField] private int critDamage;
    //Chance to get a crit
    [SerializeField] private int critChance;
    //How many bullets per shot
    //Whole numbers guarantee, decimals are a chance
    // 3.5 means 3 bullets with a 50% chance to be 4 bullets
    [SerializeField] private float multishot;
    //How many enemies a bullet may pass through
    //Whole numbers guarantee, decimals are a chance
    //3.5 means 1 shot goes through 3 enemies with a 50% chance to go through a 4th (to hit the 5th)
    [SerializeField] private float penetration;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text playerCounterText;

    private int attackCountdown;
    private int nextShotAccuracyBonus;
    private int nextShotDamageBonus;

    public int Health => currentHealth;
    public int Armor => armor;
    public int AttackCountDown => attackCountdown;
    public bool IsDead => currentHealth <= 0;


    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        attackCountdown = fireRate;
        effectText = GameObject.FindWithTag("EffectText").GetComponent<TMP_Text>();
        playerHPText = GameObject.FindWithTag("PlayerHealthText").GetComponent<TMP_Text>();
        playerCounterText = GameObject.FindWithTag("PlayerCounterText").GetComponent<TMP_Text>();
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CombatScene")
        {
            // Reset player stats when entering combat
            armor = 0;
            attackCountdown = fireRate;
            nextShotAccuracyBonus = 0;
            nextShotDamageBonus = 0;
            effectText = GameObject.FindWithTag("EffectText").GetComponent<TMP_Text>();
            playerHPText = GameObject.FindWithTag("PlayerHealthText").GetComponent<TMP_Text>();
            playerCounterText = GameObject.FindWithTag("PlayerCounterText").GetComponent<TMP_Text>();
            playerHPText.text = currentHealth.ToString();
            playerCounterText.text = attackCountdown.ToString();
        }
    }

    public void TickCountdown()
    {
        attackCountdown--;
        playerCounterText.text = attackCountdown.ToString();
    }

    public bool IsAttackReady()
    {
        return attackCountdown <= 0;
    }

    public void Attack(Enemy enemy)
    {
        int finalAccuracy = Mathf.Clamp(accuracy + nextShotAccuracyBonus, 0, 100);
        bool attackHit = Random.Range(0, 100) < finalAccuracy;

        if (attackHit)
        {
            int finalDamage = damage + nextShotDamageBonus;
            enemy.TakeDamage(finalDamage);

            Debug.Log("Player dealt " + finalDamage + " damage.");
            EffectTextQueue.Enqueue("Player dealt " + finalDamage + " damage to " + enemy.name + "!");
        }
        else
        {
            Debug.Log("Player missed!");
            EffectTextQueue.Enqueue("Player missed!");
        }

        nextShotAccuracyBonus = 0;
        nextShotDamageBonus = 0;
        attackCountdown = fireRate;
        playerCounterText.text = attackCountdown.ToString();
    }

    public void GainAim(int amount)
    {
        nextShotAccuracyBonus += amount;
        Debug.Log("next shot gained " + amount + " accuracy.");
        EffectTextQueue.Enqueue("Next shot gained " + amount + " accuracy!");
    }

    public void GainArmor(int amount)
    {
        armor += amount;
        Debug.Log("player gained " + amount + " armor.");
        EffectTextQueue.Enqueue("Player gained " + amount + " armor!");
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("PLayer healed to " + currentHealth + " health.");
        EffectTextQueue.Enqueue("Player healed to " + currentHealth + " health!");
        playerHPText.text = currentHealth.ToString();
    }

    public void Quickdraw(int amount)
    {
        attackCountdown -= amount;
        Debug.Log("Player countdown is now " + attackCountdown + ".");
        EffectTextQueue.Enqueue("Player countdown is now " + attackCountdown + "!");
        playerCounterText.text = attackCountdown.ToString();
    }

    public void LoadShot(int amount)
    {
        nextShotDamageBonus += amount;
        Debug.Log("next shot gained " + amount + " damage.");
        EffectTextQueue.Enqueue("Next shot gained " + amount + " damage!");
    }

    public void TakeDamage(int amount)
    {
        int absorbedDamage = Mathf.Min(armor, amount);

        armor -= absorbedDamage;
        amount -= absorbedDamage;
        currentHealth -= amount;

        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Player has " + currentHealth + " health and " + armor + " armor.");
        EffectTextQueue.Enqueue("Player has " + currentHealth + " health and " + armor + " armor!");
        playerHPText.text = currentHealth.ToString();
    }
}
