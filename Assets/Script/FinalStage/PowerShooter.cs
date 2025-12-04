using UnityEngine;

public class PowerShooter : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject powerProjectilePrefab;
    public float spawnDistance = 0.3f;

    [Header("State")]
    [Tooltip("Only true during the boss fight (Stage 3).")]
    public bool canShoot = false;

    [Header("Fire Settings")]
    [Tooltip("If true, shooting is limited by fireCooldown.")]
    public bool useCooldown = false;
    [Tooltip("Seconds between shots when cooldown is used.")]
    public float fireCooldown = 1.0f;

    private Camera cam;
    private float cooldownTimer = 0f;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        if (!canShoot) return;

        // Tick cooldown
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (useCooldown)
        {
            // Hard mode: only fire if cooldown has expired
            if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0f)
            {
                Shoot();
                cooldownTimer = fireCooldown;
            }
        }
        else
        {
            // Easy mode: fire on every press, no cooldown
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        if (powerProjectilePrefab == null || cam == null) return;

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
        Quaternion spawnRot = cam.transform.rotation;

        GameObject projObj = Instantiate(powerProjectilePrefab, spawnPos, spawnRot);
        Debug.Log("PowerShooter: Fired projectile.");

        PowerProjectile proj = projObj.GetComponent<PowerProjectile>();
        if (proj != null && Stage3Manager.Instance != null)
        {
            Transform bossT = Stage3Manager.Instance.GetBossTransform();
            if (bossT != null)
            {
                proj.target = bossT;
            }
        }
    }

    // Called by Stage3Manager to configure difficulty
    public void ConfigureDifficulty(bool easyMode)
    {
        if (easyMode)
        {
            useCooldown = false;       // free fire
            fireCooldown = 0.1f;       // not really used, but can be tiny
        }
        else
        {
            useCooldown = true;        // gated by cooldown
            fireCooldown = 1.0f;       // one shot per second
            cooldownTimer = 0f;        // ready to fire immediately once
        }
    }
}
