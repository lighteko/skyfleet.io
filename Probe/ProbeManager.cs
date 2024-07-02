using UnityEngine;
using Unity.Netcode;

public class ProbeManager : NetworkBehaviour
{
    private NetworkVariable<HealthState> _health;
    private Transform _lastHit, _healthBar;

    private NetworkVariable<HealthState> Health { get => _health; set => _health = value; }
    public Transform LastHit { get => _lastHit; set => _lastHit = value; }

    private void Awake()
    {
        var readPerm = NetworkVariableReadPermission.Everyone;
        var writePerm = NetworkVariableWritePermission.Server;
        _health = new(readPerm: readPerm, writePerm: writePerm);
        _healthBar = transform.GetChild(1);
    }

    public override void OnNetworkSpawn()
    {
        Health.Value = new HealthState { CurrentHP = 100, MaxHealth = 100 };
        Health.OnValueChanged += OnHealthChanged;
    }

    #region vital
    [ServerRpc]
    public void TakeDamageServerRpc(short damage)
    {
        TakeDamage(damage);
    }

    private void TakeDamage(short damage)
    {
        if (!IsServer) return;
        short HP = Health.Value.CurrentHP;
        HP -= damage;
        Health.Value = new HealthState { CurrentHP = HP, MaxHealth = Health.Value.MaxHealth };
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsOwner) return;
        Transform obj = collider.transform;
        if (obj.CompareTag("Projectile"))
        {
            _lastHit = obj.GetComponent<Projectile>().Shooter;
            short damage = obj.GetComponent<Projectile>().Damage;
            TakeDamageServerRpc(damage);
            TakeDamage(damage);
        }
    }

    private void OnHealthChanged(HealthState _, HealthState newHealth)
    {
        _healthBar.GetComponent<HealthBar>().SetHealth(newHealth.CurrentHP, newHealth.MaxHealth);
        if (newHealth.CurrentHP <= 0) DieServerRpc();
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
            var state = new ProbeDropState
            {
                Killer = _lastHit.GetComponent<PlayerStats>().OwnerClientId,
                Fuel = 100,
                Ammo = 20,
                Exp = 10
            };
            OnKilledServerRpc(state);
        }
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
    #endregion

    #region kill
    [ServerRpc]
    private void OnKilledServerRpc(ProbeDropState state)
    {
        PlayerStats killer = NetworkManager.Singleton.ConnectedClients[state.Killer].PlayerObject.GetComponent<PlayerStats>();
        killer.AddFuelServerRpc(state.Fuel);
        killer.AddAmmoServerRpc(state.Ammo);
        killer.AddExpServerRpc(state.Exp);
    }
    #endregion

    #region states
    private struct HealthState : INetworkSerializable
    {
        private short _health, _maxHealth;
        internal short CurrentHP { readonly get => _health; set => _health = value; }
        internal short MaxHealth { readonly get => _maxHealth; set => _maxHealth = value; }

        void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            serializer.SerializeValue(ref _health);
            serializer.SerializeValue(ref _maxHealth);
        }
    }

    private struct ProbeDropState : INetworkSerializable
    {
        private ulong _killer;
        private short _fuel, _ammo, _exp;

        internal ulong Killer { readonly get => _killer; set => _killer = value; }
        internal short Fuel { readonly get => _fuel; set => _fuel = value; }
        internal short Ammo { readonly get => _ammo; set => _ammo = value; }
        internal short Exp { readonly get => _exp; set => _exp = value; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _killer);
            serializer.SerializeValue(ref _fuel);
            serializer.SerializeValue(ref _ammo);
            serializer.SerializeValue(ref _exp);
        }
    }
    #endregion
}