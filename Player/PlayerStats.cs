using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    private NetworkVariable<FixedString32Bytes> _id;
    public NetworkVariable<FixedString32Bytes> Id { get => _id; set => _id = value; }

    #region stats
    private NetworkVariable<short> _health;
    private NetworkVariable<short> _fuel;
    private NetworkVariable<short> _ammo;
    private NetworkVariable<short> _atk, _def;
    private NetworkVariable<short> _maxHealth, _maxFuel, _level;
    private NetworkVariable<float> _healthRegen, _fuelEfficiency, _exp, _movementSpeed;

    public NetworkVariable<short> Health { get => _health; set => _health = value; }
    public NetworkVariable<short> Fuel { get => _fuel; set => _fuel = value; }
    public NetworkVariable<short> Ammo { get => _ammo; set => _ammo = value; }

    public NetworkVariable<short> MaxHealth { get => _maxHealth; set => _maxHealth = value; }
    public NetworkVariable<short> MaxFuel { get => _maxFuel; set => _maxFuel = value; }
    public NetworkVariable<float> HealthRegen { get => _healthRegen; set => _healthRegen = value; }
    public NetworkVariable<float> FuelEfficiency { get => _fuelEfficiency; set => _fuelEfficiency = value; }
    public NetworkVariable<float> MovementSpeed { get => _movementSpeed; set => _movementSpeed = value; }
    public NetworkVariable<float> Exp { get => _exp; set => _exp = value; }
    public NetworkVariable<short> Level { get => _level; set => _level = value; }
    public NetworkVariable<short> AttackPower { get => _atk; set => _atk = value; }
    public NetworkVariable<short> DefencePower { get => _def; set => _def = value; }

    #endregion

    [SerializeField] private bool _usingServerAuth;
    private Transform _healthBar;
    private Transform _fuelBar;
    private Transform _lastHit;

    void Awake()
    {
        _healthBar = transform.GetChild(2);
        _fuelBar = transform.GetChild(3);
        var writePerm = _usingServerAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        var readPerm = NetworkVariableReadPermission.Everyone;
        var ownerPerm = NetworkVariableReadPermission.Owner;

        _health = new(readPerm: readPerm, writePerm: writePerm);
        _atk = new(readPerm: readPerm, writePerm: writePerm);
        _def = new(readPerm: readPerm, writePerm: writePerm);
        _fuel = new(readPerm: readPerm, writePerm: writePerm);
        _ammo = new(readPerm: readPerm, writePerm: writePerm);
        _maxHealth = new(readPerm: readPerm, writePerm: writePerm);
        _maxFuel = new(readPerm: readPerm, writePerm: writePerm);
        _level = new(readPerm: readPerm, writePerm: writePerm);
        _healthRegen = new(readPerm: ownerPerm, writePerm: writePerm);
        _fuelEfficiency = new(readPerm: ownerPerm, writePerm: writePerm);
        _movementSpeed = new(readPerm: ownerPerm, writePerm: writePerm);
        _exp = new(readPerm: ownerPerm, writePerm: writePerm);
        Id = new();
    }

    void Update()
    {

    }

    // void FixedUpdate()
    // {
    //     if (!IsOwner) return;
    //     // ConsumeFuelServerRpc();
    //     // HealServerRpc();
    // }



    public override void OnNetworkSpawn()
    {
        Health.OnValueChanged += OnHealthChanged;
        if (!IsOwner) return;
        SendIdServerRpc($"Player {OwnerClientId}");
        InitializeStats();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendIdServerRpc(string id)
    {
        Id.Value = id;
    }

    private void InitializeStats()
    {
        Health.Value = MaxHealth.Value = 100;
        Fuel.Value = MaxFuel.Value = 100;
        Ammo.Value = 20;
        AttackPower.Value = 10;
        DefencePower.Value = 5;
        MovementSpeed.Value = 0.2f;
        HealthRegen.Value = 0.8f;
        FuelEfficiency.Value = 0.2f;
        Exp.Value = 0;
        Level.Value = 0;
    }

    // #region level
    // public void AddExp(float exp)
    // {
    //     Exp += exp;
    //     if (CheckLevelUp()) LevelUp();
    // }

    // private bool CheckLevelUp()
    // {
    //     return Exp >= 100 * Mathf.Pow(1.25f, Level + 1);
    // }

    // private void LevelUp()
    // {
    //     if (!IsOwner) return;
    //     Exp = 0;
    //     Level++;
    //     Health.Value = MaxHealth;
    //     Fuel.Value = MaxFuel;
    //     Debug.Log($"Player {OwnerClientId} leveled up to level {Level}");
    // }

    // #endregion

    #region ammo

    [ServerRpc]
    public void AddAmmoServerRpc(short ammo)
    {
        AddAmmoClientRpc(ammo);
    }

    [ClientRpc]
    public void AddAmmoClientRpc(short ammo)
    {
        if (!IsOwner) AddAmmo(ammo);
    }
    public void AddAmmo(short ammo)
    {
        if (!IsOwner) return;
        Ammo.Value += ammo;
    }

    [ServerRpc]
    public void ConsumeAmmoServerRpc(short ammo)
    {
        ConsumeAmmoClientRpc(ammo);
    }
    [ClientRpc]
    public void ConsumeAmmoClientRpc(short ammo)
    {
        if (!IsOwner) ConsumeAmmo(ammo);
    }
    public void ConsumeAmmo(short ammo)
    {
        if (Ammo.Value < ammo || !IsOwner) return;
        Ammo.Value -= ammo;
    }

    #endregion

    #region vitality
    [ServerRpc]
    public void RequestDamageServerRpc(short damage)
    {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    public void TakeDamageClientRpc(short damage)
    {
        if (!IsOwner) TakeDamage(damage);
    }

    private void TakeDamage(short damage)
    {
        if (!IsOwner) return;
        Health.Value -= damage;
    }

    private void OnHealthChanged(short _, short newHealth)
    {
        _healthBar.GetComponent<HealthBar>().SetHealth(newHealth, MaxHealth.Value);
        if (Health.Value <= 0) DieServerRpc();
    }
    private void OnFuelChanged(short _, short newFuel)
    {
        _fuelBar.GetComponent<FuelBar>().SetFuel(newFuel, MaxFuel.Value);
        if (Fuel.Value <= 0) DieServerRpc();
    }

    [ServerRpc]
    private void DieServerRpc()
    {
        Die();
    }

    private void Die()
    {
        if (!IsServer) return;
        if (_lastHit != null)
        {
            var state = new PlayerDropState
            {
                Id = OwnerClientId,
                Killer = _lastHit.GetComponent<PlayerStats>().OwnerClientId,
                Fuel = Fuel.Value,
                Ammo = Ammo.Value,
                Level = Level.Value,
                Exp = Exp.Value
            };
            OnKilledServerRpc(state);
        }
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsOwner) return;
        Transform obj = collider.transform;
        if (obj.CompareTag("Projectile") && obj.GetComponent<Projectile>().Shooter != transform)
        {
            _lastHit = obj.GetComponent<Projectile>().Shooter;
            short damage = obj.GetComponent<Projectile>().Damage;
            short actualDamage = (short)(damage - DefencePower.Value);
            if (damage < DefencePower.Value) return;
            RequestDamageServerRpc(actualDamage);
            TakeDamage(actualDamage);
        }
    }

    // [ServerRpc]
    // public void HealServerRpc()
    // {
    //     Health.Value += (short)(2 * (1 + HealthRegen));
    //     if (Health.Value > MaxHealth) Health.Value = MaxHealth;
    // }

    // public void Refuel(short fuel)
    // {
    //     Fuel.Value += fuel;
    //     if (Fuel.Value > MaxFuel) Fuel.Value = MaxFuel;
    // }

    // [ServerRpc]
    // public void ConsumeFuelServerRpc()
    // {
    //     Fuel.Value -= (short)(1 * (1 - FuelEfficiency));
    //     if (Fuel.Value <= 0) Die();
    // }

    #endregion

    #region kill
    [ServerRpc]
    private void OnKilledServerRpc(PlayerDropState data)
    {
        OnKilledClientRpc(data);
    }

    [ClientRpc]
    private void OnKilledClientRpc(PlayerDropState data)
    {
        if (data.Killer != OwnerClientId) return;

    }
    #endregion

    #region state
    private struct PlayerDropState : INetworkSerializable
    {
        private short _fuel, _ammo, _level;
        private float _exp;
        private ulong _id;
        private ulong _killer;

        internal ulong Killer { readonly get => _killer; set => _killer = value; }
        internal ulong Id { readonly get => _id; set => _id = value; }
        internal short Fuel { readonly get => _fuel; set => _fuel = value; }
        internal short Ammo { readonly get => _ammo; set => _ammo = value; }
        internal short Level { readonly get => _level; set => _level = value; }
        internal float Exp { readonly get => _exp; set => _exp = value; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _id);
            serializer.SerializeValue(ref _fuel);
            serializer.SerializeValue(ref _ammo);
            serializer.SerializeValue(ref _level);
            serializer.SerializeValue(ref _exp);
        }
    }
    #endregion
}
