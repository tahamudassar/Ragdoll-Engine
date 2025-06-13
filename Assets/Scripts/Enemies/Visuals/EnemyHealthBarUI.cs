using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [SerializeField] private Transform enemy;
    [SerializeField] private Image healthBarFillImage;

    private BaseEnemy enemyScript;
    private void Start()
    {
        if (enemy == null)
        {
            Debug.LogError("Enemy Transform is not assigned in the EnemyHealthBarUI script.");
            return;
        }
        enemyScript = enemy.GetComponent<BaseEnemy>();
        if (enemyScript == null)
        {
            Debug.LogError("BaseEnemy script not found on the assigned enemy.");
        }
        enemyScript.OnHit += UpdateHealthBar;
    }

    private void UpdateHealthBar()
    {
        healthBarFillImage.fillAmount=enemyScript.GetHealthNormalized();
    }
}
