using UnityEngine;

public class PlayerController : MonoBehaviour 
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 0;
    private float angle = 0;
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = ((Vector3)worldMousePosition - transform.position).normalized;
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector3 currentPosition = transform.position;
        MoveTowardMouse(currentPosition, direction, speed * 0.1f, angle); 
    }

    public void MoveTowardMouse(Vector3 pos, Vector3 dir, float speed, float angle)
    {
        transform.position = new Vector3(pos.x + dir.x * speed, pos.y + dir.y * speed, 0);
        transform.Find("Jet").transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }
}
