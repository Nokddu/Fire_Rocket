using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RocketOrbitController : MonoBehaviour
{
    [Header("궤도 회전 속도 (deg/sec)")]
    public float orbitSpeed = 90f;

    [Header("탈출 후 직선 이동 속도 (units/sec)")]
    public float launchSpeed = 5f;

    [Header("처음에 가장 가까운 궤도 자동 캡처")]
    public float startSearchRadius = 5f;

    private bool isOrbiting = false;   // 행성 주위를 도는 중인지
    private bool isFlying = false;     // 직선 비행 중인지

    private Transform orbitCenter;     // 현재 도는 행성(혹은 궤도)의 중심
    private float orbitRadius;         // 궤도 반지름
    private float orbitAngle;          // 현재 궤도에서의 각도 (degree)

    private Vector2 flyDirection;      // 직선 비행 방향

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        // 시작 지점 주변에 궤도가 있으면 자동으로 첫 행성 궤도에 붙이기
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, startSearchRadius);
        Collider2D nearestOrbit = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Orbit"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestOrbit = hit;
                }
            }
        }

        if (nearestOrbit != null)
        {
            EnterOrbit(nearestOrbit.transform);
        }
        else
        {
            // 시작 궤도 못 찾으면 일단 비행 상태로 두기
            isFlying = true;
            flyDirection = transform.up;
        }
    }

    private void Update()
    {
        if (isOrbiting)
        {
            UpdateOrbit();

            // 궤도 도는 동안 클릭하면 탈출
            if (Input.GetMouseButtonDown(0))
            {
                LaunchFromOrbit();
            }
        }
        else if (isFlying)
        {
            UpdateFlight();
        }
    }

    /// <summary>
    /// 궤도 모드에서 매 프레임 위치/각도 갱신
    /// </summary>
    private void UpdateOrbit()
    {
        // 각도 증가 (counter-clockwise 기준)
        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        // 중심 + 반지름 * (cos, sin)
        Vector2 center = orbitCenter.position;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
        Vector2 newPos = center + offset;

        transform.position = newPos;

        // 궤도 접선 방향 = (-sin, cos)
        Vector2 tangent = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        transform.up = tangent; // 로켓이 진행 방향을 바라보도록
    }

    /// <summary>
    /// 직선 비행 모드에서 매 프레임 위치 갱신
    /// </summary>
    private void UpdateFlight()
    {
        transform.position += (Vector3)(flyDirection * launchSpeed * Time.deltaTime);
        // transform.up은 이미 LaunchFromOrbit에서 설정된 방향 그대로 유지
    }

    /// <summary>
    /// 궤도에 진입했을 때 호출
    /// </summary>
    private void EnterOrbit(Transform orbitTransform)
    {
        isFlying = false;
        isOrbiting = true;

        orbitCenter = orbitTransform;

        // 현재 위치 기준으로 반지름/각도 계산
        Vector2 center = orbitCenter.position;
        Vector2 pos = transform.position;
        Vector2 offset = pos - center;

        orbitRadius = offset.magnitude;

        // 만약 정확히 궤도 위에 있지 않아도, 바로 원 위로 붙이고 싶으면:
        if (orbitRadius > 0.0001f)
        {
            offset = offset.normalized * orbitRadius;
            transform.position = center + offset;
        }

        orbitAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

        // 접선 방향으로 기체 회전
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector2 tangent = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        transform.up = tangent;
    }

    /// <summary>
    /// 궤도에서 탈출해서 직선 비행 시작
    /// </summary>
    private void LaunchFromOrbit()
    {
        if (!isOrbiting) return;

        isOrbiting = false;
        isFlying = true;

        // 이미 transform.up이 접선 방향을 보고 있으니,
        // 그 방향으로 쭉 나간다.
        flyDirection = transform.up;
    }

    /// <summary>
    /// 궤도 트리거에 닿았을 때: 비행 중이면 새 행성 궤도에 붙기
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Orbit")) return;

        // "날아가다가" 행성 궤도에 닿았을 때만 캡처
        if (isFlying)
        {
            EnterOrbit(other.transform);
        }
    }
}
