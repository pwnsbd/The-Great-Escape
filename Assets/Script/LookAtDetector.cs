using UnityEngine;
using UnityEngine.UI;  // for Image

public class LookAtDetector : MonoBehaviour
{
    [Tooltip("Index into Stage1Manager.hiddenObjects for this object")]
    public int objectIndex = 0;

    [Tooltip("Max angle (deg) from center of view to count as 'looking'")]
    public float angleThreshold = 20f;

    [Tooltip("How long the player must look at the object to count as FOUND")]
    public float requiredLookTime = 3.0f;  // 3-second rule

    [Tooltip("Camera representing player’s view (usually AR Camera)")]
    public Transform cameraTransform;

    [Header("UI")]
    [Tooltip("Circular Image set to Filled/Radial, used as progress bar")]
    public Image gazeProgressImage;

    private Stage1Manager stage1Manager;
    private float lookTimer = 0f;
    private bool alreadyFound = false;

    private void Start()
    {
        // Unity 2023+ API
        stage1Manager = Object.FindFirstObjectByType<Stage1Manager>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (gazeProgressImage != null)
        {
            gazeProgressImage.fillAmount = 0f;
            gazeProgressImage.enabled = false;  // start hidden
        }
    }

    private void Update()
    {
        if (alreadyFound) return;

        // Only check when object is visible (zone entered)
        if (!gameObject.activeInHierarchy)
        {
            ResetLook();
            return;
        }

        if (cameraTransform == null || stage1Manager == null)
        {
            ResetLook();
            return;
        }

        // Direction from camera to this object
        Vector3 toObject = transform.position - cameraTransform.position;
        float angle = Vector3.Angle(cameraTransform.forward, toObject);

        if (angle <= angleThreshold)
        {
            // Player is looking at the object
            lookTimer += Time.deltaTime;

            // Update circular progress (0..1)
            float progress = Mathf.Clamp01(lookTimer / requiredLookTime);
            UpdateProgressUI(progress);

            if (lookTimer >= requiredLookTime)
            {
                // After 3 seconds, mark as FOUND
                stage1Manager.OnObjectSeen(objectIndex);
                alreadyFound = true;
                Debug.Log($"LookAtDetector: Object {objectIndex} FOUND after {requiredLookTime} seconds of looking.");

                // Hide the progress bar
                if (gazeProgressImage != null)
                {
                    gazeProgressImage.fillAmount = 1f;
                    gazeProgressImage.enabled = false;
                }
            }
        }
        else
        {
            // Looked away → reset timer & UI
            ResetLook();
        }
    }

    private void ResetLook()
    {
        lookTimer = 0f;
        UpdateProgressUI(0f);
    }

    private void UpdateProgressUI(float value)
    {
        if (gazeProgressImage == null) return;

        // Show it only while actually looking at the object (value > 0)
        gazeProgressImage.enabled = value > 0f;
        gazeProgressImage.fillAmount = value;
    }
}
