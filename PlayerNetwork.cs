using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<PlayerNetworkData> _netState = new(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 _vel;
    private float _rotVel;
    [SerializeField] private float _cheapInterpolationTime = 0.1f;
    void FixedUpdate()
    {
        if (IsOwner)
        {
            _netState.Value = new PlayerNetworkData() {
                Position = transform.position,
                Rotation = transform.GetChild(0).rotation.eulerAngles
            };
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, _netState.Value.Position, ref _vel, _cheapInterpolationTime);
            transform.GetChild(0).rotation = Quaternion.Euler(0,0,Mathf.SmoothDampAngle(transform.GetChild(0).rotation.eulerAngles.z, _netState.Value.Rotation.z, ref _rotVel, _cheapInterpolationTime));
        }
    }
}

struct PlayerNetworkData : INetworkSerializable
{
    private float _x, _y;
    private short _zRot;

    internal Vector3 Position {
        get => new(_x, _y, 0);
        set {
            _x = value.x;
            _y = value.y;
        }
    }

    internal Vector3 Rotation {
        get => new(0,0,_zRot);
        set => _zRot = (short) value.z;
    }
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _x);
        serializer.SerializeValue(ref _y);
        serializer.SerializeValue(ref _zRot);
    }
}
