using UnityEngine;
using System;
using System.Collections;

public class DoorCalibrator : MonoBehaviour
{
    [Header("References")]
    public Transform roomRoot;
    public Transform doorAnchor;
    public Transform arCamera; 

    private Vector3 initialRoomPosition;
    private Quaternion initialRoomRotation;

    // NEW: flag + event
    public bool IsCalibrated { get; private set; } = false;
    public Action OnCalibrated;

    private void Awake()
    {
        if (roomRoot != null)
        {
            initialRoomPosition = roomRoot.position;
            initialRoomRotation = roomRoot.rotation;
        }
    }

    public void Calibrate()
    {
        if (roomRoot == null || doorAnchor == null || arCamera == null)
        {
            Debug.LogWarning("DoorCalibrator: Missing references.");
            return;
        }

        roomRoot.position = initialRoomPosition;
        roomRoot.rotation = initialRoomRotation;

        Vector3 camForward = arCamera.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 doorForward = doorAnchor.forward;
        doorForward.y = 0f;
        doorForward.Normalize();

        if (camForward.sqrMagnitude < 0.0001f || doorForward.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("DoorCalibrator: Invalid forward vectors for yaw alignment.");
            return;
        }

        float angle = Vector3.SignedAngle(doorForward, camForward, Vector3.up);
        roomRoot.Rotate(Vector3.up, angle, Space.World);

        Vector3 doorPos = doorAnchor.position;
        Vector3 camPos  = arCamera.position;

        Vector3 offset = Vector3.zero;
        offset.x = camPos.x - doorPos.x;
        offset.z = camPos.z - doorPos.z;
        offset.y = 0f;

        roomRoot.position += offset;

        IsCalibrated = true;
        Debug.Log("DoorCalibrator: Calibration complete.");
        OnCalibrated?.Invoke();

        // StartCoroutine(HideRoomMeshesAfterDelay(3f));
    }

    public void HideRoomMeshes(float delay)
    {
        StartCoroutine(HideRoomMeshesAfterDelay(delay));
    }

    private IEnumerator HideRoomMeshesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var renderer in roomRoot.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = false;
        }

        Debug.Log("DoorCalibrator: Room meshes hidden.");
    }
}
