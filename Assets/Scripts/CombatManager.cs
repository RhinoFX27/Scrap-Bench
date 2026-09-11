using UnityEngine;

public class CombatManager : MonoBehaviour
{

    [SerializeField] private Player player;
    [SerializeField] private Enemy[] enemies;

    private bool combatEnded;

    public void PerformPlayerAction(string actionName)
    {
        if (combatEnded)
            return;
        ApplyAction(actionName);
        AdvanceCombat();
        CheckForCombatEnd();
    }

    private void ApplyAction(string actionName)
    {
        switch (actionName)
        {
            case "Aim":
                player.GainAim(20);
                break;
            case "Brace":
                player.GainArmor(3);
                break;
            case "Patch":
                player.Heal(4);
                break;
            case "Quickdraw":
                player.Quickdraw(1);
                break;
            case "Suppress":
                Enemy target = GetFrontEnemy();
                if (target != null)
                    target.DelayAttack(1);
                break;
            case "Load":
                player.LoadShot(3);
                break;
            default:
                Debug.LogWarning("Unknown action: " + actionName);
                break;
        }
    }

    private void AdvanceCombat()
    {
        player.TickCountdown();

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
                enemy.TickCountdown();
        }

        //Player attacks first if coutndowns reach zero together.
        if (player.IsAttackReady())
        {
            Enemy target = GetFrontEnemy();
            if (target != null)
                player.Attack(target);
        }

        //Every surviving enemy whose countdown reached zero attacks.
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead && enemy.IsAttackReady())
            {
                enemy.Attack(player);

                if (player.IsDead)
                    break;
            }
        }
    }

    private Enemy GetFrontEnemy()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
                return enemy;
        }

        return null;
    }

    private bool AreAllEnemiesDead()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
                return false;
        }

        return true;
    }

    private void CheckForCombatEnd()
    {
        if (AreAllEnemiesDead())
        {
            combatEnded = true;
            Debug.Log("The player won!");
        }
        else if (player.IsDead)
        {
            combatEnded = true;
            Debug.Log("The player lost!");
        }
    }
}
