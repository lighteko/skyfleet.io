using UnityEngine;

public class ProbeAutoPilot : MonoBehaviour
{
    private short _angle = 0;
    private Vector3 _direction;
    private Vector3 _targetPosition;

    void Start()
    {
        _targetPosition = RandomUtils.GetRandomPosition();
    }

    void FixedUpdate()
    {
        AutoPilot();
    }

    void Update() {
        if (Vector3.Distance(transform.position, _targetPosition) < 5) ResetTargetPosition();
    }

    private void AutoPilot() {
        _direction = _targetPosition - transform.position;

        Vector3 normal = _direction.normalized;

        _angle = (short)(Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg);

        MoveToPivot(transform.position, normal, 0.05f, _angle);
    }

    private void ResetTargetPosition() {
        _targetPosition = RandomUtils.GetRandomPosition();
    }

    private void MoveToPivot(Vector3 pos, Vector3 dir, float speed, short angle)
    {
        transform.position = new Vector3(pos.x + dir.x * speed, pos.y + dir.y * speed, 0);
        transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }
}