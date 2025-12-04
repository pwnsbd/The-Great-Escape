using UnityEngine;
using System.Collections;
using TMPro;


public class Stage1Manager : MonoBehaviour
{
    [Header("References")]
    public DoorCalibrator doorCalibrator;

    [Header("Stage 1 Objects")]
    public GameObject[] hiddenObjects;  // objects that get revealed in zones
    public GameObject[] triggerZones;   // trigger volumes for each object

    [Header("UI")]
    [Tooltip("Canvas shown at the very beginning, before calibration.")]
    public GameObject calibrationUI;

    [Tooltip("Game instruction canvas used right AFTER calibration (has Play/Start button).")]
    public GameObject gameInstructionCanvas;

    [Tooltip("HUD shown while the player is collecting objects in Stage 1.")]
    public GameObject stage1HUD;

    [Tooltip("Canvas shown AFTER all Stage 1 objects are found (for second-round hints / next stage).")]
    public GameObject secondStageHintCanvas;

    [Header("Stage 2")]
    public Stage2Manager stage2Manager;

    [Header("HUD Text")]
    public TMP_Text progressText;
    public TMP_Text hintText;

    private Coroutine instructionDelayRoutine;


    // Internal state
    private bool[] foundFlags;
    private bool stage1Active = false;

    public int FoundCount { get; private set; } = 0;
    public int TotalObjects => hiddenObjects != null ? hiddenObjects.Length : 0;

    private void Awake()
    {
        // Subscribe to calibration event from DoorCalibrator
        if (doorCalibrator != null)
        {
            doorCalibrator.OnCalibrated += HandleCalibrated;
        }
    }

    private void OnDestroy()
    {
        if (doorCalibrator != null)
        {
            doorCalibrator.OnCalibrated -= HandleCalibrated;
        }
    }

    private IEnumerator ShowInstructionAfterDelay()
    {

        if (hintText != null)
        {
            hintText.text = "Adjust the room. You can press Calibrate again...";
        }


        yield return new WaitForSeconds(3f);


        if (calibrationUI != null)
            calibrationUI.SetActive(false);


        if (doorCalibrator != null)
        {
            doorCalibrator.HideRoomMeshes(0f);
        }


        if (gameInstructionCanvas != null)
            gameInstructionCanvas.SetActive(true);
    }


    private void Start()
    {

        if (hiddenObjects != null)
        {
            foreach (var obj in hiddenObjects)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }


        SetTriggerZonesActive(false);

        if (hiddenObjects != null && hiddenObjects.Length > 0)
        {
            foundFlags = new bool[hiddenObjects.Length];
        }

        // Initial UI state
        if (calibrationUI != null)          calibrationUI.SetActive(true);
        if (gameInstructionCanvas != null)  gameInstructionCanvas.SetActive(false);
        if (stage1HUD != null)              stage1HUD.SetActive(false);
        if (secondStageHintCanvas != null)  secondStageHintCanvas.SetActive(false);

        UpdateHUD();
    }


   private void HandleCalibrated()
    {
        Debug.Log("Stage1Manager: Calibration event received. Starting/resetting 5-second window...");


        if (instructionDelayRoutine != null)
        {
            StopCoroutine(instructionDelayRoutine);
        }


        instructionDelayRoutine = StartCoroutine(ShowInstructionAfterDelay());
    }


    public void OnStartGameButtonPressed()
    {
        Debug.Log("Stage1Manager: Start Game button pressed. Starting Stage 1.");

        if (gameInstructionCanvas != null)
            gameInstructionCanvas.SetActive(false);

        if (stage1HUD != null)
            stage1HUD.SetActive(true);

        StartStage1();
    }

    private void StartStage1()
    {
        if (stage1Active) return;

        stage1Active = true;
        Debug.Log("Stage1Manager: Stage 1 is now ACTIVE.");
        SetTriggerZonesActive(true);
        UpdateHUD();
    }

    private void SetTriggerZonesActive(bool active)
    {
        if (triggerZones == null) return;

        foreach (var zone in triggerZones)
        {
            if (zone != null)
                zone.SetActive(active);
        }
    }



    public void OnZoneEntered(int zoneIndex)
    {
        if (!stage1Active) return;
        if (hiddenObjects == null) return;
        if (zoneIndex < 0 || zoneIndex >= hiddenObjects.Length) return;


        if (IsObjectFound(zoneIndex))
            return;

        var obj = hiddenObjects[zoneIndex];
        if (obj != null)
            obj.SetActive(true);
    }

    public void OnZoneExited(int zoneIndex)
    {
        if (!stage1Active) return;
        if (hiddenObjects == null) return;
        if (zoneIndex < 0 || zoneIndex >= hiddenObjects.Length) return;


        if (IsObjectFound(zoneIndex))
            return;

        var obj = hiddenObjects[zoneIndex];
        if (obj != null)
            obj.SetActive(false);
    }


    public void OnObjectSeen(int objectIndex)
    {
        if (!stage1Active) return;
        if (hiddenObjects == null) return;
        if (objectIndex < 0 || objectIndex >= hiddenObjects.Length) return;

        if (foundFlags != null && !foundFlags[objectIndex])
        {
            foundFlags[objectIndex] = true;
            FoundCount++;

            Debug.Log($"Stage1Manager: Object {objectIndex} FOUND. " +
                      $"Total found: {FoundCount}/{TotalObjects}");

            // Hide the object permanently
            var obj = hiddenObjects[objectIndex];
            if (obj != null)
                obj.SetActive(false);

            // Disable its trigger zone so re-enter does nothing
            if (triggerZones != null &&
                objectIndex >= 0 &&
                objectIndex < triggerZones.Length &&
                triggerZones[objectIndex] != null)
            {
                triggerZones[objectIndex].SetActive(false);
            }

            UpdateHUD();

            if (AllObjectsFound())
            {
                HandleStage1Complete();
            }
        }
    }

    // -----------------------------------------------------------------
    //  STAGE 1 COMPLETE → SECOND STAGE HINTS
    // -----------------------------------------------------------------

    private void HandleStage1Complete()
    {
        Debug.Log("Stage1Manager: All objects found! Stage 1 COMPLETE.");

        stage1Active = false;
        SetTriggerZonesActive(false);

        if (stage1HUD != null)
            stage1HUD.SetActive(false);

        // Show your second stage canvas (hints for round 2)
        if (secondStageHintCanvas != null)
            secondStageHintCanvas.SetActive(true);

        UpdateHUD(); // keep text consistent
    }

    // -----------------------------------------------------------------
    //  HELPERS
    // -----------------------------------------------------------------

    public bool AllObjectsFound()
    {
        return TotalObjects > 0 && FoundCount == TotalObjects;
    }

    private void UpdateHUD()
    {
        if (progressText != null)
        {
            progressText.text = $"Found {FoundCount} / {TotalObjects} clues";
        }

        if (hintText != null)
        {
            if (AllObjectsFound())
            {
                hintText.text = "All clues found! Check the next hints for Stage 2.";
            }
            else if (stage1Active)
            {
                hintText.text = "Walk around and follow the sound to find clues.";
            }
            else
            {
                hintText.text = "Get ready to start the hunt.";
            }
        }
    }

    public bool IsObjectFound(int index)
    {
        if (foundFlags == null) return false;
        if (index < 0 || index >= foundFlags.Length) return false;
        return foundFlags[index];
    }

    public void OnStartStage2ButtonPressed()
    {
        Debug.Log("Stage1Manager: Start Stage 2 button pressed.");

        // Hide the Stage2 instruction / hints canvas
        if (secondStageHintCanvas != null)
            secondStageHintCanvas.SetActive(false);

        // Hand off control to Stage2Manager
        if (stage2Manager != null)
        {
            stage2Manager.BeginStage2();
        }
        else
        {
            Debug.LogWarning("Stage1Manager: Stage2Manager reference is missing!");
        }
    }

    
}
