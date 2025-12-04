using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossController : MonoBehaviour
{
    [Header("Boss Stats")]
    public int MaxHealth = 5;

    [Header("Aiming / Tracking")]
    [Tooltip("How often (in seconds) the boss updates its aim toward the player. " +
             "Smaller = more accurate tracking, larger = easier to dodge.")]
    public float aimUpdateInterval = 0.1f;  // ~every few frames

    [Header("Projectile Attack")]
    [Tooltip("Projectile prefab the boss throws toward the player.")]
    public GameObject bossProjectilePrefab;

    [Tooltip("Where the projectile spawns (usually in front of boss chest/hand).")]
    public Transform projectileSpawnPoint;

    [Tooltip("Seconds between boss shots.")]
    public float shotInterval = 2.0f;

    public int projectileDamage = 1;

    private int currentHealth;
    private float aimTimer = 0f;
    private float shotTimer = 0f;

    private Transform playerCamera;
    private Stage3Manager stage3Manager;
    [Header("Targeting")]
public Transform projectileTarget;

    public int CurrentHealth => currentHealth;

    public void Initialize(Stage3Manager mgr, Transform playerCam)
    {
        stage3Manager = mgr;
        playerCamera = playerCam;
        currentHealth = MaxHealth;

        var col = GetComponent<Collider>();
        col.isTrigger = true;


        aimTimer = 0f;
        shotTimer = shotInterval;
    }

    private void Update()
    {
        if (playerCamera == null) return;

        aimTimer += Time.deltaTime;
        if (aimTimer >= aimUpdateInterval)
        {
            aimTimer = 0f;
            UpdateAimTowardPlayer();
        }


        shotTimer -= Time.deltaTime;
        if (shotTimer <= 0f)
        {
            shotTimer = shotInterval;
            ShootProjectile();
        }
    }

    private void UpdateAimTowardPlayer()
    {
        Vector3 toPlayer = playerCamera.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            transform.rotation = lookRot;
        }
    }

    private void ShootProjectile()
    {
        if (bossProjectilePrefab == null) return;


        Vector3 spawnPos = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + transform.forward * 0.3f;

        Quaternion spawnRot = transform.rotation;

        GameObject projObj = Instantiate(bossProjectilePrefab, spawnPos, spawnRot);

        BossProjectile proj = projObj.GetComponent<BossProjectile>();
        if (proj != null)
        {
            proj.damage = projectileDamage;
        }

        Debug.Log("BossController: Fired projectile toward player.");
    }


    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"BossController: Took {amount} damage. HP = {currentHealth}/{MaxHealth}");

        if (stage3Manager != null)
        {
            stage3Manager.UpdateBossHealthUI();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("BossController: Boss defeated!");

        if (stage3Manager != null)
        {
            stage3Manager.OnBossDefeated();
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {

        PowerProjectile proj = other.GetComponent<PowerProjectile>();
        if (proj != null)
        {
            TakeDamage(proj.damage);
            Destroy(other.gameObject);
        }
    }
}
