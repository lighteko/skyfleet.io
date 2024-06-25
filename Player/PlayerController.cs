using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed;
    private short _angle = 0;
    private Vector3 _direction;
    private Vector2 _worldMousePosition;

    public float Speed { get => _speed; set => _speed = value; }

    void FixedUpdate()
    {
        Vector2 newMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // when the user does not move the mouse, keep the direction.
        if (newMousePosition != _worldMousePosition) {
            _worldMousePosition = newMousePosition;
            _direction = (Vector3) _worldMousePosition - transform.position;
        }
        
        Vector3 normal = _direction.normalized;

        _angle = (short)(Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg);

        MoveTowardMouse(transform.position, normal, Speed * 0.01f, _angle);
    }

    public void MoveTowardMouse(Vector3 pos, Vector3 dir, float speed, short angle)
    {
        transform.position = new Vector3(pos.x + dir.x * speed, pos.y + dir.y * speed, 0);
        transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }
}
