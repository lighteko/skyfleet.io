using Unity.Netcode;
using UnityEngine;

public class ProbeSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnNPC;
    }

    private void SpawnNPC()
    {
        GameObject npc = Instantiate(npcPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        npc.GetComponent<NetworkObject>().Spawn();
    }
}