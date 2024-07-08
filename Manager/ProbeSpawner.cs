using Unity.Netcode;
using UnityEngine;

public class ProbeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _probe;
    private static short _counter = 0;
    private bool _initiated = false;
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnProbes;
    }

    void Update() {
        if (_counter < 20 && _initiated) SpawnProbe();
    }

    private void SpawnProbes()
    {
        for (int i = 0; i < 20; i++)
        {
            SpawnProbe();
        }
        _initiated = true;
    }
    private void SpawnProbe()
    {
        GameObject probe = Instantiate(_probe, RandomUtils.GetRandomPosition(), Quaternion.identity);
        probe.GetComponent<NetworkObject>().Spawn();
        _counter++;
    }
}