using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    private NetworkVariable<PlayerStatsState> _playerStatsState;
    private NetworkVariable<FixedString32Bytes> _id;
    public NetworkVariable<FixedString32Bytes> Id { get => _id; set => _id = value; }
    [SerializeField] private bool _usingServerAuth;

    void Awake()
    {
        var writePerm = _usingServerAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        _playerStatsState = new(readPerm: NetworkVariableReadPermission.Everyone, writePerm: writePerm);
        Id = new();
    }

    public override void OnNetworkSpawn()
    {
        SendIdServerRpc($"Player {OwnerClientId}");
        if (IsOwner)
        {
            InitializeStats();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendIdServerRpc(string id)
    {
        Id.Value = id;
    }

    private void InitializeStats()
    {
        var stats = new PlayerStatsState {
            Health = 100,
            Fuel = 100,
            MaxHealth = 100,
            MaxFuel = 100,
            Ammo = 100,
            MovementSpeed = 0.8f,
            HealthRegen = 1,
            FuelEfficiency = 0.2f,
            Exp = 0,
        };
        _playerStatsState.Value = stats;
    }

    private struct PlayerStatsState : INetworkSerializable
    {
        private int _health, _fuel, _ammo;
        private int _maxHealth, _maxFuel;
        private float _healthRegen, _fuelEfficiency, _exp, _movementSpeed;

        public int Health { readonly get => _health; set => _health = value; }
        public int Fuel { readonly get => _fuel; set => _fuel = value; }
        public int Ammo { readonly get => _ammo; set => _ammo = value; }
        public int MaxHealth { readonly get => _maxHealth; set => _maxHealth = value; }
        public int MaxFuel { readonly get => _maxFuel; set => _maxFuel = value; }
        public float MovementSpeed { readonly get => _movementSpeed; set => _movementSpeed = value; }
        public float HealthRegen { readonly get => _healthRegen; set => _healthRegen = value; }
        public float FuelEfficiency { readonly get => _fuelEfficiency; set => _fuelEfficiency = value; }
        public float Exp { readonly get => _exp; set => _exp = value; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _health);
            serializer.SerializeValue(ref _fuel);
            serializer.SerializeValue(ref _ammo);
            serializer.SerializeValue(ref _maxHealth);
            serializer.SerializeValue(ref _maxFuel);
            serializer.SerializeValue(ref _healthRegen);
            serializer.SerializeValue(ref _fuelEfficiency);
            serializer.SerializeValue(ref _movementSpeed);
            serializer.SerializeValue(ref _exp);
        }
    }
}
