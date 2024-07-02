using Unity.Netcode;
using UnityEngine;

public class ProbeSpawner : MonoBehaviour
{
    public GameObject probe;
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnProbe;
    }

    private void SpawnProbe()
    {
        GameObject npc = Instantiate(probe, new Vector3(0, 0, 0), Quaternion.identity);
        npc.GetComponent<NetworkObject>().Spawn();
    }
}