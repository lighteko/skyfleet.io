using UnityEngine;
using Unity.Netcode;

public class PlayerTransform : NetworkBehaviour
{
    private NetworkVariable<PlayerTransformState> _playerState;
    private Vector3 _vel;
    private float _rotVel;
    [SerializeField] private bool _usingServerAuth;
    [SerializeField] private float _cheapInterpolationTime = 0.1f;
    private GameObject _jet;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _jet = transform.GetChild(0).gameObject;
        var permission = _usingServerAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        _playerState = new(writePerm: permission);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Destroy(transform.GetComponent<PlayerController>());
            Destroy(transform.GetChild(3).gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner) TransmitState();
        else ConsumeState();
    }

    private void TransmitState()
    {
        var state = new PlayerTransformState
        {
            Position = _rb.position,
            Rotation = _jet.transform.rotation.eulerAngles
        };
        if (IsServer || !_usingServerAuth) _playerState.Value = state;
        else TransmitStateServerRpc(state);
    }

    [ServerRpc]
    private void TransmitStateServerRpc(PlayerTransformState state)
    {
        _playerState.Value = state;
    }

    private void ConsumeState()
    {
        // TODO: Need to change the interpolation method here
        _rb.MovePosition(Vector3.SmoothDamp(transform.position, _playerState.Value.Position, ref _vel, _cheapInterpolationTime));

        _jet.transform.rotation = Quaternion.Euler(0, 0, Mathf.SmoothDampAngle(_jet.transform.rotation.eulerAngles.z, _playerState.Value.Rotation.z, ref _rotVel, _cheapInterpolationTime));
    }

    private struct PlayerTransformState : INetworkSerializable
    {
        private float _x, _y;
        private short _zRot;

        internal Vector3 Position
        {
            readonly get => new(_x, _y, 0);
            set
            {
                _x = value.x;
                _y = value.y;
            }
        }

        internal Vector3 Rotation
        {
            readonly get => new(0, 0, _zRot);
            set => _zRot = (short)value.z;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _x);
            serializer.SerializeValue(ref _y);
            serializer.SerializeValue(ref _zRot);
        }
    }
}
