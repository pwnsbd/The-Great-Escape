using UnityEngine;
using Vuforia;
using System.Collections;

public class Stage2VuforiaWatcher : MonoBehaviour
{
    [Tooltip("All Vuforia ObserverBehaviours (ModelTargets / ImageTargets) for Stage 2.")]
    public ObserverBehaviour[] targets;

    [Tooltip("Index for each target into Stage2Manager.objectNames/clueTexts. " +
             "If empty or different size, we fallback to array index (0..N-1).")]
    public int[] indices;

    private Stage2Manager stage2Manager;
    private bool[] reported;

    private void Start()
    {
        stage2Manager = FindFirstObjectByType<Stage2Manager>();
        if (stage2Manager == null)
        {
            Debug.LogError("Stage2VuforiaWatcher: No Stage2Manager found in scene.");
            enabled = false;
            return;
        }

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("Stage2VuforiaWatcher: No targets assigned.");
            enabled = false;
            return;
        }

        reported = new bool[targets.Length];

        // Start polling all targets every frame
        StartCoroutine(CheckTargetsLoop());
    }

    private IEnumerator CheckTargetsLoop()
    {
        while (true)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (reported[i]) continue;        // already counted
                var ob = targets[i];
                if (ob == null) continue;

                var status = ob.TargetStatus.Status;

                // "Seen" when tracked/extended-tracked
                if (status == Status.TRACKED || status == Status.EXTENDED_TRACKED)
                {
                    reported[i] = true;

                    int objectIndex;
                    if (indices != null && indices.Length == targets.Length)
                    {
                        objectIndex = indices[i]; // explicit mapping
                    }
                    else
                    {
                        objectIndex = i; // fallback: same order as arrays in Stage2Manager
                    }

                    Debug.Log($"Stage2VuforiaWatcher: Target {i} (objectIndex {objectIndex}) recognized.");

                    // Stage2Manager itself will ignore if Stage 2 is not active yet
                    stage2Manager.OnObjectRecognized(objectIndex);
                }
            }

            yield return null; // wait for next frame
        }
    }
}
