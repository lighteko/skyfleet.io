using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private Projectile _bullet;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _cooldown = 0.5f;
    [SerializeField] private Transform _spawner;
    private float _lastFired = float.MinValue;
    private Camera _playerCam;
    private PlayerStats _playerStats;
    void Start()
    {
        _playerCam = transform.GetChild(3).GetChild(0).GetComponent<Camera>();
        _playerStats = GetComponent<PlayerStats>();
    }
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetMouseButtonDown(0) && _lastFired + _cooldown < Time.time)
        {
            _lastFired = Time.time;
            Vector2 worldMousePos = _playerCam.ScreenToWorldPoint(Input.mousePosition);
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

    private void ExecuteShoot(Vector3 dir)
    {
        if (_playerStats.Ammo.Value <= 0)
        {
            Debug.Log("No ammo left");
            return;
        }
        var bullet = Instantiate(_bullet, _spawner.position, Quaternion.identity);
        bullet.Initialize(transform, Projectile.ProjectileType.Bullet, _bulletSpeed);
        bullet.Shoot(dir);
        if (IsOwner)
        {
            _playerStats.ConsumeAmmoServerRpc(1);
        }
    }
}
