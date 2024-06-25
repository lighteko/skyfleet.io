using UnityEngine;

public class Bullet : MonoBehaviour, IShootable
{
    private Transform _shooter;

    public Transform Shooter { get => _shooter; set => _shooter = value; }

    public void Shoot(Vector3 direction, float speed)
    {
        GetComponent<Rigidbody2D>().AddForce(speed * direction);
        Invoke(nameof(DestroyProjectile), 3);
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.transform != _shooter && collider.transform.CompareTag("Player")) DestroyProjectile();
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}