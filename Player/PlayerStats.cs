using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    private NetworkVariable<FixedString32Bytes> _id;
    public NetworkVariable<FixedString32Bytes> Id { get => _id; set => _id = value; }

    #region stats
    private NetworkVariable<short> _health;
    private NetworkVariable<float> _fuel;
    private NetworkVariable<short> _ammo;
    private NetworkVariable<short> _atk, _def;
    private NetworkVariable<short> _maxHealth, _maxFuel, _level;
    private NetworkVariable<float> _healthRegen, _fuelEfficiency, _exp, _movementSpeed;

    public NetworkVariable<short> Health { get => _health; set => _health = value; }
    public NetworkVariable<float> Fuel { get => _fuel; set => _fuel = value; }
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

    private Transform _healthBar;
    private Transform _fuelBar;
    private Transform _levelBar;
    private Transform _ammoBar;
    private ulong _lastHit = ulong.MaxValue;

    #region initialize
    void Awake()
    {
        _healthBar = transform.GetChild(1);
        _fuelBar = transform.GetChild(2);
        _levelBar = transform.GetChild(3).GetChild(1);
        _ammoBar = transform.GetChild(3).GetChild(2);
        var writePerm = NetworkVariableWritePermission.Owner;
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
        Fuel.OnValueChanged += OnFuelChanged;
        if (!IsOwner) return;
        SendIdServerRpc($"Player {OwnerClientId}");
        InitializeStats();
        InvokeRepeating(nameof(ConsumeFuelServerRpc), 1, 0.3f + FuelEfficiency.Value);
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
        Ammo.OnValueChanged += OnAmmoChanged;
        Ammo.Value = 20;
        AttackPower.Value = 10;
        DefencePower.Value = 5;
        MovementSpeed.Value = 0.5f;
        HealthRegen.Value = 0.8f;
        FuelEfficiency.Value = 0.2f;
        Exp.Value = 0;
        Level.Value = 0;

        Exp.OnValueChanged += OnExpChanged;
        Level.OnValueChanged += OnLevelChanged;
    }
    #endregion

    #region level
    [ServerRpc(RequireOwnership = false)]
    public void AddExpServerRpc(float exp)
    {
        AddExpClientRpc(exp);
    }

    [ClientRpc]
    private void AddExpClientRpc(float exp)
    {
        if (IsOwner) AddExp(exp);
    }

    private void AddExp(float exp)
    {
        Exp.Value += exp;
    }

    private void OnExpChanged(float _, float newExp)
    {
        float threshold = 100 * Mathf.Pow(1.25f, Level.Value + 1);
        if (newExp >= threshold)
        {
            Level.Value++;
            Exp.Value = newExp - threshold;
        }
        _levelBar.GetComponent<LevelBar>().SetExp(Level.Value, newExp, threshold);
    }

    private void OnLevelChanged(short _, short newLevel)
    {
        float threshold = 100 * Mathf.Pow(1.25f, newLevel + 1);
        if (newLevel % 5 == 0)
        {
            // TODO: Special upgrade
        }
        short maxHP = MaxHealth.Value;
        short maxFuel = MaxFuel.Value;
        short atk = AttackPower.Value;
        short def = DefencePower.Value;

        MaxHealth.Value = (short)(maxHP + 50 * newLevel);
        MaxFuel.Value = (short)(maxFuel + 25 * newLevel);
        AttackPower.Value = (short)(atk + 10);
        DefencePower.Value = (short)(def + 10);

        _levelBar.GetComponent<LevelBar>().SetExp(newLevel, Exp.Value, threshold);
    }

    #endregion

    #region ammo

    [ServerRpc(RequireOwnership = false)]
    public void AddAmmoServerRpc(short ammo)
    {
        AddAmmoClientRpc(ammo);
    }
    [ClientRpc]
    private void AddAmmoClientRpc(short ammo)
    {
        if (IsOwner) AddAmmo(ammo);
    }
    private void AddAmmo(short ammo)
    {
        Ammo.Value += ammo;
    }

    [ServerRpc]
    public void ConsumeAmmoServerRpc(short ammo)
    {
        ConsumeAmmoClientRpc(ammo);
    }
    [ClientRpc]
    private void ConsumeAmmoClientRpc(short ammo)
    {
        if (IsOwner) ConsumeAmmo(ammo);
    }
    private void ConsumeAmmo(short ammo)
    {
        if (Ammo.Value < ammo) return;
        Ammo.Value -= ammo;
    }

    public void OnAmmoChanged(short _, short newAmmo)
    {
        _ammoBar.GetComponent<AmmoBar>().SetAmmo(newAmmo, 300);
    }

    #endregion

    #region health
    [ServerRpc]
    public void TakeDamageServerRpc(short damage)
    {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(short damage)
    {
        if (IsOwner) TakeDamage(damage);
    }

    private void TakeDamage(short damage)
    {
        Health.Value -= damage;
    }

    private void OnHealthChanged(short _, short newHealth)
    {
        _healthBar.GetComponent<HealthBar>().SetHealth(newHealth, MaxHealth.Value);
        if (!IsOwner) return;
        if (Health.Value <= 0)
        {
            var state = new PlayerDropState
            {
                Id = OwnerClientId,
                Killer = _lastHit,
                Fuel = Fuel.Value,
                Ammo = Ammo.Value,
                Level = Level.Value,
                Exp = Exp.Value
            };
            DieServerRpc(state);
        }
    }
    #endregion

    #region fuel
    [ServerRpc]
    public void ConsumeFuelServerRpc()
    {
        ConsumeFuelClientRpc();
    }

    [ClientRpc]
    private void ConsumeFuelClientRpc()
    {
        if (IsOwner) ConsumeFuel();
    }

    private void ConsumeFuel()
    {
        Fuel.Value -= 2;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddFuelServerRpc(float fuel)
    {
        AddFuelClientRpc(fuel);
    }

    [ClientRpc]
    private void AddFuelClientRpc(float fuel)
    {
        if (IsOwner) AddFuel(fuel);
    }
    private void AddFuel(float fuel)
    {
        if (Fuel.Value + fuel > MaxFuel.Value) Fuel.Value = MaxFuel.Value;
        else Fuel.Value += fuel;
    }
    private void OnFuelChanged(float _, float newFuel)
    {
        _fuelBar.GetComponent<FuelBar>().SetFuel(newFuel, MaxFuel.Value);
        if (!IsOwner) return;
        if (Fuel.Value <= 0)
        {
            var state = new PlayerDropState
            {
                Id = OwnerClientId,
                Killer = _lastHit,
                Fuel = Fuel.Value,
                Ammo = Ammo.Value,
                Level = Level.Value,
                Exp = Exp.Value
            };
            DieServerRpc(state);
        }
    }
    #endregion

    #region death
    [ServerRpc]
    private void DieServerRpc(PlayerDropState state)
    {
        Die(state);
    }

    private void Die(PlayerDropState state)
    {

        if (state.Killer != ulong.MaxValue)
        {
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
            _lastHit = obj.GetComponent<Projectile>().Shooter.GetComponent<PlayerStats>().OwnerClientId;
            short damage = obj.GetComponent<Projectile>().Damage;
            short actualDamage = (short)(damage - DefencePower.Value);
            if (damage < DefencePower.Value) return;
            TakeDamageServerRpc(actualDamage);
        }
    }
    #endregion

    #region kill
    [ServerRpc(RequireOwnership = false)]
    private void OnKilledServerRpc(PlayerDropState data)
    {
        if (data.Killer == ulong.MaxValue) return;
        var killer = NetworkManager.Singleton.ConnectedClients[data.Killer].PlayerObject.GetComponent<PlayerStats>();
        killer.AddExpServerRpc(data.Exp + 50 * (1 + data.Level));
        killer.AddFuelServerRpc(data.Fuel);
        killer.AddAmmoServerRpc(data.Ammo);
    }

    #endregion

    #region state
    private struct PlayerDropState : INetworkSerializable
    {
        private short  _ammo, _level;
        private float _exp, _fuel;
        private ulong _id;
        private ulong _killer;

        internal ulong Killer { readonly get => _killer; set => _killer = value; }
        internal ulong Id { readonly get => _id; set => _id = value; }
        internal float Fuel { readonly get => _fuel; set => _fuel = value; }
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
        public override readonly string ToString()
        {
            return $"Id: {_id}, Fuel: {_fuel}, Ammo: {_ammo}, Level: {_level}, Exp: {_exp}, Killer: {_killer}";
        }
    }
    #endregion
}
