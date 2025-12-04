using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerZone : MonoBehaviour
{
    [Tooltip("Index into Stage1Manager.hiddenObjects")]
    public int zoneIndex = 0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip proximityClip;

    private Stage1Manager stage1Manager;

    private void Start()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        stage1Manager = Object.FindFirstObjectByType<Stage1Manager>();
        if (stage1Manager == null)
        {
            Debug.LogWarning("TriggerZone: No Stage1Manager found in scene.");
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.loop = true;  // so the clue keeps playing while inside
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (stage1Manager == null) return;

        // 🔹 If this object is already found, kill this trigger and bail
        if (stage1Manager.IsObjectFound(zoneIndex))
        {
            gameObject.SetActive(false);  // optional clean-up
            return;
        }

        stage1Manager.OnZoneEntered(zoneIndex);

        if (audioSource != null && proximityClip != null)
        {
            if (audioSource.clip != proximityClip)
                audioSource.clip = proximityClip;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (stage1Manager == null) return;

        if (stage1Manager.IsObjectFound(zoneIndex))
        {
            // Already found; object/trigger should be off anyway
            return;
        }

        stage1Manager.OnZoneExited(zoneIndex);

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

}
