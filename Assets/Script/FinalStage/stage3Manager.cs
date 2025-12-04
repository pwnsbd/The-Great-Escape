using UnityEngine;
using TMPro;

public class Stage3Manager : MonoBehaviour
{
    public static Stage3Manager Instance { get; private set; }
    [Header("Boss Setup")]
    public GameObject bossPrefab;
    public Transform playerCamera;
    public float bossForwardOffset = 0.5f;

    [Header("Player Attack")]
    public PowerShooter powerShooter;

    [Header("Stage 3 HUD")]
    public GameObject stage3HUDCanvas;
    public TMP_Text bossHealthText;
    public TMP_Text stage3HintText;

    [Header("Stage 3 Intro UI")]
    public GameObject stage3IntroCanvas;

    [Header("State")]
    public bool bossRecognitionEnabled = false;


    [Header("Difficulty Settings")]
    public int easyBossHealth = 50;
    public int hardBossHealth = 100;
    public int easyPlayerHealth = 100;
    public int hardPlayerHealth = 50;

    [Header("Boss Damage")]
    public int easyBossProjectileDamage = 1;
    public int hardBossProjectileDamage = 3;

    [Header("Game Over UI")]
    public GameObject gameOverCanvas;
    public TMP_Text gameOverText;


    private bool easyMode = false;
    private PlayerHealth playerHealth;
    private Stage2Manager stage2Manager;

    private GameObject currentBoss;
    private BossController bossController;
    private bool bossFightStarted = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        stage2Manager = FindFirstObjectByType<Stage2Manager>();
    }


    public Transform GetBossTransform()
    {
        if (bossController == null)
            return null;

        // If a custom hitbox target is assigned, use that
        if (bossController.projectileTarget != null)
            return bossController.projectileTarget;

        // Fallback: boss root
        return bossController.transform;
    }


    private void Start()
    {
        if (stage3HUDCanvas != null) stage3HUDCanvas.SetActive(false);
        if (powerShooter != null) powerShooter.canShoot = false;
        if (stage3IntroCanvas != null) stage3IntroCanvas.SetActive(false);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);

        // Find PlayerHealth under the player camera
        if (playerCamera != null)
        {
            playerHealth = playerCamera.GetComponentInChildren<PlayerHealth>();
        }
    }


    public void ShowStage3IntroUI()
    {
        bossRecognitionEnabled = false;
        if (stage3HUDCanvas != null) stage3HUDCanvas.SetActive(false);
        if (stage3IntroCanvas != null) stage3IntroCanvas.SetActive(true);
        if (powerShooter != null) powerShooter.canShoot = false;
    }

    public void StartStage3()
    {
        // Decide difficulty based on Stage 2 result
        // Use your "all found" flag here – currently Stage2Success.
        bool allFound = (stage2Manager != null && stage2Manager.Stage2Success);

        easyMode = allFound;

        // Configure player HP for this mode
        if (playerHealth != null)
        {
            int hp = easyMode ? easyPlayerHealth : hardPlayerHealth;
            playerHealth.SetMaxHealth(hp);
        }

        // Existing UI / shooter setup
        bossRecognitionEnabled = true;
        if (stage3IntroCanvas != null) stage3IntroCanvas.SetActive(false);
        if (stage3HUDCanvas != null) stage3HUDCanvas.SetActive(true);

        if (powerShooter != null)
        {
            powerShooter.ConfigureDifficulty(easyMode);
            powerShooter.canShoot = true;
        }
    }


   public void BeginBossFightAt(Transform summonRoot)
    {
        if (bossFightStarted) return;
        bossFightStarted = true;

        // Try to use an existing boss under the marker
        BossController existing = summonRoot.GetComponentInChildren<BossController>(true);
        if (existing != null)
        {
            currentBoss = existing.gameObject;
            currentBoss.SetActive(true);

            // 1) Place boss at marker + forward offset
            Vector3 basePos = summonRoot.position + summonRoot.forward * bossForwardOffset;
            existing.transform.position = basePos;

            // 2) Rotate boss to face the player horizontally
            if (playerCamera != null)
            {
                Vector3 toPlayer = playerCamera.position - existing.transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude < 0.0001f)
                    toPlayer = -summonRoot.forward;

                existing.transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
            }

            bossController = existing;

            // Apply difficulty to boss HP and damage before Initialize
            int bossHp = easyMode ? easyBossHealth : hardBossHealth;
            int bossDmg = easyMode ? easyBossProjectileDamage : hardBossProjectileDamage;

            bossController.MaxHealth = bossHp;
            bossController.projectileDamage = bossDmg;

            bossController.Initialize(this, playerCamera);
            
        }
        else if (bossPrefab != null)
        {
            Vector3 spawnPos = summonRoot.position + summonRoot.forward * bossForwardOffset;
            Vector3 toPlayer = (playerCamera != null)
                ? (playerCamera.position - spawnPos)
                : summonRoot.forward * -1f;

            if (toPlayer.sqrMagnitude < 0.0001f)
                toPlayer = summonRoot.forward * -1f;

            Quaternion spawnRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);

            currentBoss = Instantiate(bossPrefab, spawnPos, spawnRot);
            bossController = currentBoss.GetComponent<BossController>();
            if (bossController != null)
            {
                int bossHp = easyMode ? easyBossHealth : hardBossHealth;
                int bossDmg = easyMode ? easyBossProjectileDamage : hardBossProjectileDamage;

                bossController.MaxHealth = bossHp;
                bossController.projectileDamage = bossDmg;

                bossController.Initialize(this, playerCamera);
            }

        }

        if (stage3HintText != null) stage3HintText.text = "The boss appeared!";
        UpdateBossHealthUI();
    }


    public void UpdateBossHealthUI()
    {
        if (bossHealthText == null || bossController == null) return;
        bossHealthText.text = $"Boss HP: {bossController.CurrentHealth} / {bossController.MaxHealth}";
    }

    public void OnBossDefeated()
    {
        ShowGameOver("You defeated the boss! 🎉");
    }

    public void OnPlayerDefeated()
    {
        ShowGameOver("The boss defeated you... 😵");
    }


    private void ShowGameOver(string message)
    {
        // Stop gameplay
        bossRecognitionEnabled = false;

        if (powerShooter != null)
            powerShooter.canShoot = false;

        if (stage3HUDCanvas != null)
            stage3HUDCanvas.SetActive(false);

        // Show game over UI
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = message;
    }

}
