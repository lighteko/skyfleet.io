using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerMovement : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 0;
    private float angle = 0;
    public NetworkVariable<FixedString32Bytes> nickName = new();
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        SendNickNameServerRpc($"Player {OwnerClientId}");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = ((Vector3)worldMousePosition - transform.position).normalized;
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector3 currentPosition = transform.position;
        SendMovementServerRpc(currentPosition, direction, speed * 0.1f, angle); 
    }

    [ServerRpc]
    public void SendMovementServerRpc(Vector3 pos, Vector3 dir, float speed, float angle)
    {
        transform.position = new Vector3(pos.x + dir.x * speed, pos.y + dir.y * speed, 0);
        transform.Find("Jet").transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }

    [ServerRpc]
    public void SendNickNameServerRpc(string nickName) {
        this.nickName.Value = nickName;
    }   
}
