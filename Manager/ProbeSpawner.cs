using Unity.Netcode;
using UnityEngine;

public class ProbeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _probe;
    private static short _counter = 0;
    private bool _initiated = false;
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnProbe;
    }

    void FixedUpdate() {
        if (_counter < 5 && _initiated) SpawnProbe();
    }
    private void SpawnProbe()
    {
        Debug.Log("Spawning probe");
        GameObject probe = Instantiate(_probe, RandomUtils.GetRandomPosition(), Quaternion.identity);
        probe.GetComponent<NetworkObject>().Spawn();
        _counter++;
        _initiated = true;
    }
}