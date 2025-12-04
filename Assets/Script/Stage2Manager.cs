using UnityEngine;
using TMPro;

public class Stage2Manager : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Time limit (seconds) to find all physical objects.")]
    public float timeLimitSeconds = 60f;

    [Header("UI Canvases")]
    [Tooltip("Instruction canvas shown BEFORE Stage 2 starts (you already made this).")]
    public GameObject stage2InstructionCanvas;

    [Tooltip("HUD canvas shown DURING Stage 2 (timer + progress).")]
    public GameObject stage2HUDCanvas;

    [Header("HUD Text")]
    public TMP_Text timerText;
    public TMP_Text progressText;
    public TMP_Text resultText; // Optional, can show success/fail message

    [Header("Stage 2 Objects")]
    [Tooltip("One entry per physical object you track with Vuforia.")]
    public string[] objectNames;  // just for display

    [Tooltip("One TMP_Text per clue line in the UI (cross off when found).")]
    public TMP_Text[] clueTexts;

    public GameObject[] objectsToDestroyAfterStage2;

    [Tooltip("Sound played whenever a Stage 2 object is found.")]
    public AudioSource objectFoundSound;


    [Header("Stage 3 Hook")]
    [Tooltip("Root GameObject that has Stage3Manager on it. Should be DISABLED at startup.")]
    public Stage3Manager stage3Manager;

    // --- internal state ---
    private bool[] foundFlags;
    private float remainingTime;
    private bool stage2Active = false;

    public bool Stage2Completed { get; private set; } = false;
    public bool Stage2Success   { get; private set; } = false;

    public int FoundCount { get; private set; } = 0;
    public int TotalObjects => (objectNames != null) ? objectNames.Length : 0;


    public bool Stage2Active => stage2Active;

    private void Start()
    {
      
        if (objectNames != null && objectNames.Length > 0)
        {
            foundFlags = new bool[objectNames.Length];
        }

        if (stage2InstructionCanvas != null)
            stage2InstructionCanvas.SetActive(false);

        if (stage2HUDCanvas != null)
            stage2HUDCanvas.SetActive(false);

        UpdateProgressUI();
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!stage2Active) return;
        if (Stage2Completed) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            HandleTimeout();
        }

        UpdateTimerUI();
    }

    public void BeginStage2()
    {
        Debug.Log("Stage2Manager: BeginStage2 called. Starting timer and enabling HUD.");

        Stage2Completed = false;
        Stage2Success   = false;
        stage2Active    = true;
        FoundCount      = 0;


        if (foundFlags != null)
        {
            for (int i = 0; i < foundFlags.Length; i++)
                foundFlags[i] = false;
        }

        remainingTime = timeLimitSeconds;

        if (stage2InstructionCanvas != null)
            stage2InstructionCanvas.SetActive(false);

        if (stage2HUDCanvas != null)
            stage2HUDCanvas.SetActive(true);

        if (resultText != null)
            resultText.text = "";

        UpdateTimerUI();
        UpdateProgressUI();
    }

    public void OnObjectRecognized(int index)
    {
        if (!stage2Active) return;
        if (Stage2Completed) return;
        if (foundFlags == null) return;
        if (index < 0 || index >= foundFlags.Length) return;

        if (foundFlags[index])
        {
            return;
        }

        foundFlags[index] = true;
        FoundCount++;

        Debug.Log($"Stage2Manager: Object {index} recognized. Found {FoundCount}/{TotalObjects}");

        if (objectFoundSound != null)
        {
            objectFoundSound.Play();
        }

        if (clueTexts != null &&
            index >= 0 && index < clueTexts.Length &&
            clueTexts[index] != null)
        {

            string name = (objectNames != null && index < objectNames.Length)
                ? objectNames[index]
                : $"Object {index}";
            clueTexts[index].text = $"<s>{name}</s>";
        }

        UpdateProgressUI();

        if (FoundCount == TotalObjects && TotalObjects > 0)
        {
            HandleSuccess();
        }
    }


    private void HandleSuccess()
    {
        Stage2Completed = true;
        Stage2Success   = true;
        stage2Active    = false;

        foreach (var go in objectsToDestroyAfterStage2)
            if (go != null)
                Destroy(go);

        if (resultText != null)
            resultText.text = "You found all the treasures in time!";

        if (stage3Manager != null)
        {
            stage2HUDCanvas.SetActive(false);
            stage3Manager.gameObject.SetActive(true);
            stage3Manager.ShowStage3IntroUI();
        }
    }


    private void HandleTimeout()
    {
        if (Stage2Completed) return;

        Stage2Completed = true;
        Stage2Success   = false;
        stage2Active    = false;

        Debug.Log("Stage2Manager: Time is up! Stage 2 FAILED.");

        if (resultText != null)
            resultText.text = "Time is up! You did not find all the objects.";

        if (stage3Manager != null)
        {
            stage2HUDCanvas.SetActive(false);
            stage3Manager.gameObject.SetActive(true);
            stage3Manager.ShowStage3IntroUI();
        }
    }



    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
        timerText.text = $"Time: {seconds}s";
    }

    private void UpdateProgressUI()
    {
        if (progressText != null)
        {
            progressText.text = $"Found {FoundCount} / {TotalObjects} objects";
        }


        if (objectNames != null && clueTexts != null)
        {
            for (int i = 0; i < clueTexts.Length && i < objectNames.Length; i++)
            {
                if (clueTexts[i] == null) continue;

                if (!Stage2Completed && (foundFlags == null || !foundFlags[i]))
                {
                    clueTexts[i].text = objectNames[i];
                }
            }
        }
    }
}
