using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private short _angle = 0;
    private Vector3 _direction;
    private Vector2 _worldMousePosition;
    private Camera _playerCam;
    private PlayerStats _playerStats;

    void Start()
    {
        _playerCam = transform.GetChild(1).GetComponent<Camera>();
        _playerStats = GetComponent<PlayerStats>();
    }

    void FixedUpdate()
    {
        _worldMousePosition = _playerCam.ScreenToWorldPoint(Input.mousePosition); ;
        _direction = (Vector3)_worldMousePosition - transform.position;

        Vector3 normal = _direction.normalized;

        _angle = (short)(Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg);

        MoveTowardMouse(transform.position, normal, _playerStats.MovementSpeed * 0.1f, _angle);
    }

    public void MoveTowardMouse(Vector3 pos, Vector3 dir, float speed, short angle)
    {
        transform.position = new Vector3(pos.x + dir.x * speed, pos.y + dir.y * speed, 0);
        transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }
}
