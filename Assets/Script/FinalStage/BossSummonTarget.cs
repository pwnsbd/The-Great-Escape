using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ObserverBehaviour))]
public class BossSummonTarget : MonoBehaviour
{
    public Stage3Manager stage3Manager;
    public bool requireStage2Success = true;

    private Stage2Manager stage2Manager;
    private ObserverBehaviour observer;
    private bool bossSpawned = false;

    private void Start()
    {
        observer = GetComponent<ObserverBehaviour>();
        observer.OnTargetStatusChanged += OnStatusChanged;

        stage2Manager = FindFirstObjectByType<Stage2Manager>();
    }

    private void OnDestroy()
    {
        if (observer != null)
            observer.OnTargetStatusChanged -= OnStatusChanged;
    }

    private void Update()
    {
        TrySpawnBossIfReady(null);
    }

    private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        TrySpawnBossIfReady(status.Status);
    }

    private void TrySpawnBossIfReady(Status? overrideStatus)
    {
        if (bossSpawned) return;
        if (stage3Manager == null) return;


        if (requireStage2Success && stage2Manager != null && !stage2Manager.Stage2Success)
            return;

        if (!stage3Manager.bossRecognitionEnabled)
            return;

        
        Status s = overrideStatus ?? observer.TargetStatus.Status;

        if (s == Status.TRACKED || s == Status.EXTENDED_TRACKED)
        {
            bossSpawned = true;
            Debug.Log("BossSummonTarget: Boss marker tracked — spawning boss.");
            stage3Manager.BeginBossFightAt(transform);
        }
    }
}
