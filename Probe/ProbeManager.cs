using UnityEngine;
using Unity.Netcode;

public class ProbeManager : NetworkBehaviour
{
    private NetworkVariable<HealthState> _health;
    private NetworkVariable<ProbeTransformState> _probeState;
    private Transform _lastHit, _healthBar;
    private GameObject _jet;
    private Rigidbody2D _rb;
    [SerializeField] private float _cheapInterpolationTime = 0.1f;
    private Vector3 _vel;
    private float _rotVel;

    private NetworkVariable<HealthState> Health { get => _health; set => _health = value; }
    public Transform LastHit { get => _lastHit; set => _lastHit = value; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _jet = transform.GetChild(0).gameObject;
        _healthBar = transform.GetChild(1);
        var readPerm = NetworkVariableReadPermission.Everyone;
        var writePerm = NetworkVariableWritePermission.Server;
        var permission = NetworkVariableWritePermission.Server;
        _health = new(readPerm: readPerm, writePerm: writePerm);
        _probeState = new(writePerm: permission);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) Health.Value = new HealthState { CurrentHP = 100, MaxHealth = 100 };
        else
        {
            Destroy(transform.GetComponent<ProbeAutoPilot>());
        }
        Health.OnValueChanged += OnHealthChanged;
    }

    private void FixedUpdate()
    {
        if (IsServer) TransmitState();
        else ConsumeState();
    }

    private void TransmitState()
    {
        var state = new ProbeTransformState
        {
            Position = _rb.position,
            Rotation = _jet.transform.rotation.eulerAngles
        };
        _probeState.Value = state;
    }

    private void ConsumeState()
    {
        // TODO: Need to change the interpolation method here
        _rb.MovePosition(Vector3.SmoothDamp(transform.position, _probeState.Value.Position, ref _vel, _cheapInterpolationTime));
        _jet.transform.rotation = Quaternion.Euler(0, 0, Mathf.SmoothDampAngle(_jet.transform.rotation.eulerAngles.z, _probeState.Value.Rotation.z, ref _rotVel, _cheapInterpolationTime));
    }

    #region vital

    private void TakeDamage(short damage)
    {
        short HP = Health.Value.CurrentHP;
        HP -= damage;
        Health.Value = new HealthState { CurrentHP = HP, MaxHealth = Health.Value.MaxHealth };
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer) return;
        Transform obj = collider.transform;
        if (obj.CompareTag("Projectile"))
        {
            _lastHit = obj.GetComponent<Projectile>().Shooter;
            short damage = obj.GetComponent<Projectile>().Damage;
            TakeDamage(damage);
        }
    }

    private void OnHealthChanged(HealthState _, HealthState newHealth)
    {
        _healthBar.GetComponent<HealthBar>().SetHealth(newHealth.CurrentHP, newHealth.MaxHealth);
        if (newHealth.CurrentHP <= 0 && IsServer) Die();
    }

    private void Die()
    {
        if (_lastHit != null)
        {
            var state = new ProbeDropState
            {
                Killer = _lastHit.GetComponent<PlayerStats>().OwnerClientId,
                Fuel = 50,
                Ammo = 20,
                Exp = 10
            };
            OnKilled(state);
        }
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
    #endregion

    #region kill
    private void OnKilled(ProbeDropState state)
    {
        Debug.Log("Called");
        Debug.Log(state);
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

        public override readonly string ToString()
        {
            return $"Killer: {_killer}, Fuel: {_fuel}, Ammo: {_ammo}, Exp: {_exp}";
        }
    }
    private struct ProbeTransformState : INetworkSerializable
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
    #endregion
}