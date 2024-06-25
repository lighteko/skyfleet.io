using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private Bullet _bullet;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _cooldown = 0.5f;
    [SerializeField] private Transform _spawner;

    private float _lastFired = float.MinValue;
    private bool _fired;
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
            StartCoroutine(ToggleLagIndicator());
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
            Debug.Log(collider.transform.GetComponent<Bullet>().Shooter.GetComponent<PlayerTransform>().Id.Value);
    }

    private void ExecuteShoot(Vector3 dir)
    {
        var bullet = Instantiate(_bullet, _spawner.position, Quaternion.identity);
        bullet.Shooter = transform;
        bullet.Shoot(dir, _bulletSpeed);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (_fired) GUILayout.Label("FIRED LOCALLY");

        GUILayout.EndArea();
    }

    private struct PlayerShootingState : INetworkSerializable
    {
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            throw new System.NotImplementedException();
        }
    }

    private IEnumerator ToggleLagIndicator()
    {
        _fired = true;
        yield return new WaitForSeconds(0.2f);
        _fired = false;
    }
}
