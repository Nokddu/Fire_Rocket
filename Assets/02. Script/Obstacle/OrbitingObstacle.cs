using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OrbitingObstacle : MonoBehaviour
{
    [Header("Center(궤도 중심)")]
    [SerializeField] private Transform center;

    [Header("공전 반지름 (궤도 반지름 기준)")]
    [SerializeField] private float orbitRadiusMultiplier = 1.15f; // 궤도보다 "조금" 크게
    [SerializeField] private float orbitRadiusAdd = 0f;

    [Header("회전")]
    [SerializeField] private float angularSpeed = 90f;  // deg/sec
    [SerializeField] private int direction = 1;         // 1=반시계, -1=시계
    [SerializeField] private float startAngle = 0f;

    [Header("충돌 옵션")]
    [SerializeField] private bool restartOnHit = false;

    [Header("Collider 없을 때 대체 반지름")]
    [SerializeField] private float fallbackBaseRadius = 1f;

    private float orbitRadiusWorld;
    private float angle;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Initialize(
        Transform orbitCenter,
        float startAngleDeg,
        int dir,
        float speedDeg,
        float radiusMul,
        float radiusAdd,
        bool restart)
    {
        center = orbitCenter;
        startAngle = startAngleDeg;
        angle = startAngleDeg;

        direction = dir == 0 ? 1 : (dir > 0 ? 1 : -1);
        angularSpeed = speedDeg;

        orbitRadiusMultiplier = radiusMul;
        orbitRadiusAdd = radiusAdd;

        restartOnHit = restart;

        RecalculateRadius();
        UpdatePosition();
    }

    private void RecalculateRadius()
    {
        if (center == null)
        {
            orbitRadiusWorld = fallbackBaseRadius;
            return;
        }

        float baseRadius = fallbackBaseRadius;

        // 로켓이 궤도 반지름 잡는 방식과 동일: CircleCollider2D.radius * lossyScale
        // (RocketOrbitController.UpdateOrbit/EnterOrbit에서 쓰는 계산) :contentReference[oaicite:0]{index=0}
        var circle = center.GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            Vector3 scale = center.lossyScale;
            float maxScale = Mathf.Max(scale.x, scale.y);
            baseRadius = circle.radius * maxScale;
        }

        orbitRadiusWorld = baseRadius * orbitRadiusMultiplier + orbitRadiusAdd;
    }

    private void Update()
    {
        if (center == null)
        {
            Destroy(gameObject);
            return;
        }

        angle += direction * angularSpeed * Time.deltaTime;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector3 c = center.position;
        transform.position = c + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadiusWorld;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameManager.Instance.GameOver();

        //var rocket = other.GetComponentInParent<RocketOrbitController>();
        //if (rocket == null) return;

        //rocket.HitByObstacle(restartOnHit);
    }
}
