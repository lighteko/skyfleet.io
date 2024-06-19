using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        MoveTowardsMouse();
    }

    void MoveTowardsMouse()
    {
        Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = ((Vector3)worldMousePosition - transform.position).normalized;
        Vector3 currentPosition = transform.position;
        transform.position = new Vector3(currentPosition.x + direction.x * speed, currentPosition.y + direction.y * speed, 0);
    }

}
