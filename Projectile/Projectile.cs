using UnityEngine;

public class Projectile : MonoBehaviour, IShootable
{
    private Transform _shooter;

    public Transform Shooter { get => _shooter; set => _shooter = value; }

    private short _damage;
    private short _range = 0;
    private float _speed;
    private Vector3 _startPos;
    public short Damage { get => _damage; private set => _damage = value; }
    public short Range { get => _range; private set => _range = value; }
    public float Speed { get => _speed; private set => _speed = value; }
    public enum ProjectileType { Null, Bullet, Missile }
    public ProjectileType type = ProjectileType.Null;
    private bool _initialized = false;

    public void Initialize(Transform shooter, ProjectileType p_type, float speed)
    {
        Shooter = shooter;
        type = p_type;
        short atk = shooter.GetComponent<PlayerStats>().AttackPower.Value;
        switch (p_type)
        {
            case ProjectileType.Bullet:
                Damage = atk;
                Range = 5;
                Speed = 700 * speed;
                break;
            case ProjectileType.Missile:
                Damage = (short)(atk * 2);
                Range = 20;
                Speed = 2000;
                break;
            default:
                break;
        }
        _initialized = true;
    }
    public void Update()
    {
        if (IsOutOfRange())
        {
            DestroyProjectile();
        }
    }
    public bool IsOutOfRange()
    {
        if (_startPos == null) return false;
        return Vector3.Distance(_startPos, transform.position) > Range;
    }

    public void Shoot(Vector3 direction)
    {
        if (!_initialized) return;
        _startPos = transform.position;
        GetComponent<Rigidbody2D>().AddForce(Speed * direction);
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag("Wall")) DestroyProjectile();
        if (col.transform != _shooter && col.transform.CompareTag("Player"))
            DestroyProjectile();
        if (col.transform.CompareTag("Probe"))
            DestroyProjectile();
    }
}