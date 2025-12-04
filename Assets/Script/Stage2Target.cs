using UnityEngine;
using TMPro;
using Vuforia;

[RequireComponent(typeof(ObserverBehaviour))]
public class Stage2Target : MonoBehaviour
{
    public int stage2Index;
    public Stage2Manager stage2Manager;

    private ObserverBehaviour observer;
    private bool hasReported = false;

    void Start()
    {
        observer = GetComponent<ObserverBehaviour>();
        if (stage2Manager == null)
            stage2Manager = FindFirstObjectByType<Stage2Manager>();

        observer.OnTargetStatusChanged += OnStatusChanged;
    }

    void OnDestroy()
    {
        if (observer != null)
            observer.OnTargetStatusChanged -= OnStatusChanged;
    }

    private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        if (hasReported) return;
        if (!stage2Manager.Stage2Active) return; // if you expose that bool

        var s = status.Status;
        if (s == Status.TRACKED || s == Status.EXTENDED_TRACKED)
        {
            hasReported = true;
            stage2Manager.OnObjectRecognized(stage2Index);
        }
    }
}
