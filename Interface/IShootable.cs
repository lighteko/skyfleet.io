using UnityEngine;

public interface IShootable
{
    void Shoot(Vector3 direction);
    void DestroyProjectile();
    void OnTriggerEnter2D(Collider2D col);
}