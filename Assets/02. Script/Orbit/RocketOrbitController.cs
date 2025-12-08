using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RocketOrbitController : MonoBehaviour
{
    [Header("궤도 이동 설정")]
    public float orbitSpeed = 60f;          // 궤도에서 도는 각속도 (deg/sec)
    public float launchSpeed = 6f;          // 탈출 후 직선 속도

    [Header("부드러움 설정")]
    public float radiusLerpSpeed = 5f;      // 궤도 반지름으로 붙는 속도
    public float rotationLerpSpeed = 10f;   // 접선 방향으로 돌아가는 속도

    [Header("시작 시 가장 가까운 궤도 찾기 반경")]
    public float startSearchRadius = 5f;

    private bool isOrbiting = false;
    private bool isFlying = false;

    private Transform orbitCenter;          // 현재 도는 궤도의 중심(행성 위치)
    private float orbitAngle;               // 궤도상의 각도 (degree)
    private float orbitDirection = 1f;      // +1 = 반시계, -1 = 시계

    private float currentRadius;            // 현재 실제 반지름 (계속 보간됨)
    private float targetRadius;             // 궤도 콜라이더에서 가져온 목표 반지름

    private Vector2 flyDirection;           // 직선 비행 방향

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;      // 힘/중력 안 씀
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        // 시작 위치 근처에서 가장 가까운 Orbit 찾기
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, startSearchRadius);
        Collider2D nearestOrbit = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Orbit")) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestOrbit = hit;
            }
        }

        if (nearestOrbit != null)
        {
            EnterOrbit(nearestOrbit.transform);
        }
        else
        {
            // 시작 궤도가 없으면 그냥 직선 비행
            isFlying = true;
            flyDirection = transform.up;
        }
    }

    private void Update()
    {
        if (isOrbiting)
        {
            UpdateOrbit();

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
    /// 궤도 모드: 행성 주위를 부드럽게 도는 로직
    /// </summary>
    private void UpdateOrbit()
    {
        if (orbitCenter == null) return;

        // 각도 진행
        orbitAngle += orbitDirection * orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        // 현재 반지름을 목표 반지름으로 서서히 보간 (나선형으로 붙는 느낌)
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, radiusLerpSpeed * Time.deltaTime);

        Vector2 center = orbitCenter.position;
        Vector2 dirOnCircle = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 newPos = center + dirOnCircle * currentRadius;

        transform.position = newPos;

        // 접선 방향(반시계 기준)
        Vector2 baseTangent = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        // 좌/우 회전에 따라 뒤집기
        Vector2 tangent = baseTangent * orbitDirection;
        Vector3 targetUp = tangent.normalized;

        // 방향을 항상 타겟 접선 방향으로 부드럽게 회전
        if (targetUp.sqrMagnitude > 0.0001f)
        {
            transform.up = Vector3.Slerp(
                transform.up,
                targetUp,
                rotationLerpSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// 직선 비행 모드
    /// </summary>
    private void UpdateFlight()
    {
        transform.position += (Vector3)(flyDirection * launchSpeed * Time.deltaTime);
        // 방향은 그 상태 유지
    }

    /// <summary>
    /// 궤도에 진입 (Orbit 태그 가진 오브젝트의 Transform)
    /// </summary>
    private void EnterOrbit(Transform orbitTransform)
    {
        isFlying = false;
        isOrbiting = true;

        orbitCenter = orbitTransform;

        // 1) 궤도 목표 반지름: CircleCollider2D.radius × scale
        float radiusFromCollider = 1f;
        CircleCollider2D circle = orbitTransform.GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            Vector3 scale = orbitTransform.lossyScale;
            float maxScale = Mathf.Max(scale.x, scale.y);
            radiusFromCollider = circle.radius * maxScale;
        }
        targetRadius = radiusFromCollider;

        // 2) 현재 위치 기준으로 현재 반지름/각도 계산 (스냅 X)
        Vector2 center = orbitCenter.position;
        Vector2 offset = (Vector2)transform.position - center;
        currentRadius = offset.magnitude;

        if (currentRadius < 0.01f)
        {
            // 혹시 중심에 너무 가까우면 임의 방향으로 빼주기
            currentRadius = targetRadius * 0.5f;
            offset = Vector2.right * currentRadius;
            transform.position = center + offset;
        }

        orbitAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        float rad = orbitAngle * Mathf.Deg2Rad;

        // 3) 이 각도에서의 접선 방향 둘 다 구해서,
        //    지금 로켓이 바라보는 방향과 더 가까운 쪽으로 회전 방향 선택
        Vector2 tangentCCW = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad)); // 반시계(왼쪽)
        Vector2 tangentCW = new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)); // 시계(오른쪽)

        Vector2 forward = transform.up.normalized;
        float dotCCW = Vector2.Dot(forward, tangentCCW);
        float dotCW = Vector2.Dot(forward, tangentCW);

        orbitDirection = (dotCCW >= dotCW) ? 1f : -1f;
    }

    /// <summary>
    /// 궤도에서 탈출 → 현재 바라보는 방향으로 직선 비행
    /// </summary>
    private void LaunchFromOrbit()
    {
        if (!isOrbiting) return;

        isOrbiting = false;
        isFlying = true;

        flyDirection = transform.up.normalized;
    }

    /// <summary>
    /// Orbit 트리거에 닿았을 때: 직선 비행 중이면 새 궤도에 진입
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Orbit")) return;

        if (isFlying)
        {
            EnterOrbit(other.transform);
        }
    }
}
