using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    private NetworkVariable<FixedString32Bytes> _id;
    public NetworkVariable<FixedString32Bytes> Id { get => _id; set => _id = value; }

    #region stats
    private NetworkVariable<short> _health, _fuel, _ammo, _maxHealth, _maxFuel, _level;
    private NetworkVariable<float> _healthRegen, _fuelEfficiency, _exp, _movementSpeed;

    public NetworkVariable<short> Health { get => _health; set => _health = value; }
    public NetworkVariable<short> Fuel { get => _fuel; set => _fuel = value; }
    public NetworkVariable<short> Ammo { get => _ammo; set => _ammo = value; }
    public NetworkVariable<short> MaxHealth { get => _maxHealth; set => _maxHealth = value; }
    public NetworkVariable<short> MaxFuel { get => _maxFuel; set => _maxFuel = value; }
    public NetworkVariable<float> HealthRegen { get => _healthRegen; set => _healthRegen = value; }
    public NetworkVariable<float> FuelEfficiency { get => _fuelEfficiency; set => _fuelEfficiency = value; }
    public NetworkVariable<float> Exp { get => _exp; set => _exp = value; }
    public NetworkVariable<float> MovementSpeed { get => _movementSpeed; set => _movementSpeed = value; }
    public NetworkVariable<short> Level { get => _level; set => _level = value; }

    #endregion

    [SerializeField] private bool _usingServerAuth;

    void Awake()
    {
        var writePerm = _usingServerAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        var readPerm = NetworkVariableReadPermission.Everyone;

        _health = new(readPerm: readPerm, writePerm: writePerm);
        _fuel = new(readPerm: readPerm, writePerm: writePerm);
        _ammo = new(readPerm: readPerm, writePerm: writePerm);
        _maxHealth = new(readPerm: readPerm, writePerm: writePerm);
        _maxFuel = new(readPerm: readPerm, writePerm: writePerm);
        _level = new(readPerm: readPerm, writePerm: writePerm);

        _healthRegen = new(readPerm: readPerm, writePerm: writePerm);
        _fuelEfficiency = new(readPerm: readPerm, writePerm: writePerm);
        _exp = new(readPerm: readPerm, writePerm: writePerm);
        _movementSpeed = new(readPerm: readPerm, writePerm: writePerm);

        Id = new();
    }

    public override void OnNetworkSpawn()
    {
        SendIdServerRpc($"Player {OwnerClientId}");
        if (IsOwner)
        {
            InitializeStatsServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendIdServerRpc(string id)
    {
        Id.Value = id;
    }

    [ServerRpc]
    private void InitializeStatsServerRpc()
    {
        Health.Value = 100;
        Fuel.Value = 100;
        MaxHealth.Value = 100;
        MaxFuel.Value = 100;
        Ammo.Value = 100;
        MovementSpeed.Value = 0.8f;
        HealthRegen.Value = 1;
        FuelEfficiency.Value = 0.2f;
        Exp.Value = 0;
        Level.Value = 0;
    }

    
}
