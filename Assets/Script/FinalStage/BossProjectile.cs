using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossProjectile : MonoBehaviour
{
    public float speed = 3f;
    public float lifeTime = 5f;
    public int damage = 1;

    private Vector3 direction;

    private void Start()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;


        direction = transform.forward;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

}
