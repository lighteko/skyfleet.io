using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private Bullet _bullet;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _cooldown = 0.5f;
    [SerializeField] private Transform _spawner;
    private float _lastFired = float.MinValue;
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetMouseButtonDown(0) && _lastFired + _cooldown < Time.time)
        {
            _lastFired = Time.time;
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var dir = (Vector3)worldMousePos - transform.position;
            dir = dir.normalized;
            RequestFireServerRpc(dir);

            ExecuteShoot(dir);
        }
    }

    [ServerRpc]
    private void RequestFireServerRpc(Vector3 dir)
    {
        FireClientRpc(dir);
    }

    [ClientRpc]
    private void FireClientRpc(Vector3 dir)
    {
        if (!IsOwner) ExecuteShoot(dir);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.transform.CompareTag("Projectile"))
            Debug.Log(collider.transform.GetComponent<Bullet>().Shooter.GetComponent<PlayerStats>().Id.Value);
    }

    private void ExecuteShoot(Vector3 dir)
    {
        var bullet = Instantiate(_bullet, _spawner.position, Quaternion.identity);
        bullet.Shooter = transform;
        bullet.Shoot(dir, _bulletSpeed);
    }
}
