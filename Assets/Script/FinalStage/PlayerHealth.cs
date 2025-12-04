using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public TMP_Text healthText;    // Assign from Stage3 HUD
    public GameObject damageFlash; // Optional UI effect, assign in inspector

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        BossProjectile proj = other.GetComponent<BossProjectile>();
        if (proj != null)
        {
            TakeDamage(proj.damage);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();
        PlayDamageFlash();

        Debug.Log($"Player took {amount} damage! HP = {currentHealth}/{maxHealth}");

        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
    }

    private void PlayDamageFlash()
    {
        if (damageFlash == null) return;

        // briefly enable a red flash UI
        damageFlash.SetActive(true);
        CancelInvoke(nameof(HideFlash));
        Invoke(nameof(HideFlash), 0.2f);
    }

    private void HideFlash()
    {
        damageFlash.SetActive(false);
    }


    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = maxHealth;
        UpdateUI();
    }

    private void Die()
    {
        if (Stage3Manager.Instance != null)
        {
            Stage3Manager.Instance.OnPlayerDefeated();
        }
    }
}
