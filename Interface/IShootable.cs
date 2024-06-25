using UnityEngine;

public interface IShootable {
    void Shoot(Vector3 direction, float speed);
    void DestroyProjectile();
}