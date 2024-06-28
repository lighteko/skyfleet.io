using UnityEngine;

public class Projectile : MonoBehaviour, IShootable
{
    private Transform _shooter;

    public Transform Shooter { get => _shooter; set => _shooter = value; }

    private short _damage;
    public short Damage { get => _damage; set => _damage = value; }

    public void Shoot(Vector3 direction, float speed)
    {
        GetComponent<Rigidbody2D>().AddForce(speed * direction);
        Invoke(nameof(DestroyProjectile), 3);
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D col)
    {   
        if (col.transform.CompareTag("Wall")) DestroyProjectile();
        if (col.transform != _shooter && col.transform.CompareTag("Player"))
            DestroyProjectile();

    }
}