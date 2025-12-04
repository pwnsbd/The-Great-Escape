using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PowerProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifeTime = 3f;
    public int damage = 1;

    [Header("Homing")]
    public Transform target;
    public float rotateSpeed = 10f;   // only for visual rotation

    private void Start()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target != null)
        {
            // 1) Move DIRECTLY toward the boss (position-based, not "forward"-based)
            Vector3 toTarget = target.position - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;

                // If we would step past the target, clamp to exact position
                if (distanceThisFrame * distanceThisFrame >= toTarget.sqrMagnitude)
                {
                    transform.position = target.position;
                }
                else
                {
                    transform.position += dir * distanceThisFrame;
                }

                // 2) Rotate just for visual orientation (optional)
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRot,
                    rotateSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // Fallback: no target → just go forward
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}
