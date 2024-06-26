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

    public override void OnNetworkSpawn() {
        if (!IsOwner) Destroy(transform.GetChild(1).gameObject);
    }
    void Start()
    {
        _playerCam = transform.GetChild(1).GetComponent<Camera>();
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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Transform obj = collider.transform;
        if (obj.CompareTag("Projectile") && obj.GetComponent<Projectile>().Shooter != transform)
            Debug.Log(obj.GetComponent<Projectile>().Shooter.GetComponent<PlayerStats>().Id.Value);
    }

    private void ExecuteShoot(Vector3 dir)
    {
        var bullet = Instantiate(_bullet, _spawner.position, Quaternion.identity);
        bullet.Shooter = transform;
        bullet.Shoot(dir, _bulletSpeed);
    }
}
