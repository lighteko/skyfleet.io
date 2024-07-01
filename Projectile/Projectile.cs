using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour, IShootable
{
    private Transform _shooter;

    public Transform Shooter { get => _shooter; set => _shooter = value; }

    private short _damage;
    private short _range = 10;
    private Vector3 _startPos;
    public short Damage { get => _damage; set => _damage = value; }
    public short Range { get => _range; set => _range = value; }

    public void Update()
    {
        if (IsOutOfRange()) DestroyProjectile();
    }
    public bool IsOutOfRange()
    {
        if (_startPos == null) return false;
        return Vector3.Distance(_startPos, transform.position) > Range;
    }

    public void Shoot(Vector3 direction, float speed)
    {
        GetComponent<Rigidbody2D>().AddForce(speed * direction);
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