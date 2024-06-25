using UnityEngine;

public interface IShootable
{
    void Shoot(Vector3 direction, float speed);
    void DestroyProjectile();
    void OnTriggerEnter2D(Collider2D col);
}