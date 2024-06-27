using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    private NetworkVariable<FixedString32Bytes> _id;
    public NetworkVariable<FixedString32Bytes> Id { get => _id; set => _id = value; }

    #region stats
    private NetworkVariable<short> _health, _fuel, _ammo, _maxHealth, _maxFuel, _level;
    private NetworkVariable<float> _healthRegen, _fuelEfficiency, _exp, _movementSpeed, _atk, _def;

    public NetworkVariable<short> Health { get => _health; set => _health = value; }
    public NetworkVariable<short> Fuel { get => _fuel; set => _fuel = value; }
    public NetworkVariable<short> MaxHealth { get => _maxHealth; set => _maxHealth = value; }
    public NetworkVariable<short> MaxFuel { get => _maxFuel; set => _maxFuel = value; }
    public NetworkVariable<float> HealthRegen { get => _healthRegen; set => _healthRegen = value; }
    public NetworkVariable<float> FuelEfficiency { get => _fuelEfficiency; set => _fuelEfficiency = value; }
    public NetworkVariable<float> MovementSpeed { get => _movementSpeed; set => _movementSpeed = value; }
    
    public NetworkVariable<float> Exp { get => _exp; set => _exp = value; }
    public NetworkVariable<float> AttackPower { get => _atk; set => _atk = value; }
    public NetworkVariable<float> DefencePower { get => _def; set => _def = value; }
    public NetworkVariable<short> Level { get => _level; set => _level = value; }
    public NetworkVariable<short> Ammo { get => _ammo; set => _ammo = value; }

    #endregion

    [SerializeField] private bool _usingServerAuth;

    void Awake()
    {
        var writePerm = _usingServerAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        var readPerm = NetworkVariableReadPermission.Everyone;
        var ownerPerm = NetworkVariableReadPermission.Owner;

        _health = new(readPerm: readPerm, writePerm: writePerm);
        _fuel = new(readPerm: readPerm, writePerm: writePerm);
        _maxHealth = new(readPerm: readPerm, writePerm: writePerm);
        _maxFuel = new(readPerm: readPerm, writePerm: writePerm);
        _movementSpeed = new(readPerm: readPerm, writePerm: writePerm);
        _atk = new(readPerm: readPerm, writePerm: writePerm);
        _def = new(readPerm: readPerm, writePerm: writePerm);

        _exp = new(readPerm: ownerPerm, writePerm: writePerm);
        _level = new(readPerm: ownerPerm, writePerm: writePerm);
        _healthRegen = new(readPerm: ownerPerm, writePerm: writePerm);
        _fuelEfficiency = new(readPerm: ownerPerm, writePerm: writePerm);
        _ammo = new(readPerm: ownerPerm, writePerm: writePerm);
        Id = new();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        // ConsumeFuelServerRpc();
        // HealServerRpc();
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
        AttackPower.Value = 10;
        DefencePower.Value = 5;
        MovementSpeed.Value = 0.3f;
        HealthRegen.Value = 0.8f;
        FuelEfficiency.Value = 0.2f;
        Exp.Value = 0;
        Level.Value = 0;
    }

    #region level
    public void AddExp(float exp)
    {
        Exp.Value += exp;
        if (CheckLevelUp()) LevelUp();
    }

    private bool CheckLevelUp()
    {
        return Exp.Value >= 100 * Mathf.Pow(1.25f, Level.Value + 1);
    }

    private void LevelUp()
    {
        if (!IsOwner) return;
        Exp.Value = 0;
        Level.Value++;
        Health.Value = MaxHealth.Value;
        Fuel.Value = MaxFuel.Value;
        Debug.Log($"Player {OwnerClientId} leveled up to level {Level.Value}");
    }

    #endregion

    #region vitality
    [ServerRpc]
    public void TakeDamageServerRpc(short damage)
    {
        TakeDamage(damage);
    }

    private void TakeDamage(short damage) {
        Health.Value -= damage;
        Debug.Log($"Player {OwnerClientId} took {damage} damage: {Health.Value} health remaining.");
        if (Health.Value <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsOwner) return;
        Transform obj = collider.transform;
        if (obj.CompareTag("Projectile") && obj.GetComponent<Projectile>().Shooter != transform)
            Debug.Log(obj.GetComponent<Projectile>().Shooter.GetComponent<PlayerStats>().Id.Value);
            TakeDamageServerRpc((short)(obj.GetComponent<Projectile>().Damage - DefencePower.Value));
    }

    [ServerRpc]
    public void HealServerRpc()
    {
        Health.Value += (short)(2 * (1 + HealthRegen.Value));
        if (Health.Value > MaxHealth.Value) Health.Value = MaxHealth.Value;
    }

    public void Refuel(short fuel)
    {
        Fuel.Value += fuel;
        if (Fuel.Value > MaxFuel.Value) Fuel.Value = MaxFuel.Value;
    }

    [ServerRpc]
    public void ConsumeFuelServerRpc()
    {
        Fuel.Value -= (short)(1 * (1 - FuelEfficiency.Value));
        if (Fuel.Value <= 0) Die();
    }

    #endregion
}
