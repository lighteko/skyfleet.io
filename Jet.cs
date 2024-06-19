using System;
using UnityEngine;

public class Jet : MonoBehaviour
{
    void FixedUpdate()
    {
        rotateToMouseCursor();
    }

    void rotateToMouseCursor()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }
}
