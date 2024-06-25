using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float speed = 0;
    private short angle = 0;

    public float Speed { get => speed; set => speed = value; }

    void FixedUpdate()
    {
        Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = ((Vector3)worldMousePosition - transform.position).normalized;
        angle = (short)(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        Vector3 currentPosition = transform.position;
        MoveTowardMouse(currentPosition, direction, Speed * 0.1f, angle);
    }

    public void MoveTowardMouse(Vector3 pos, Vector3 dir, float speed, short angle)
    {
        transform.position = new Vector3(pos.x + dir.x * speed, pos.y + dir.y * speed, 0);
        transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }
}
