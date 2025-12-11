using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RocketOrbitController : MonoBehaviour
{
    [Header("궤도 이동 설정")]
    [Tooltip("행성 주위를 도는 각속도 (deg/sec)")]
    public float orbitSpeed = 60f;

    [Tooltip("궤도에서 탈출한 후 직선 이동 속도 (units/sec)")]
    public float launchSpeed = 6f;

    [Header("부드러움 설정")]
    [Tooltip("현재 반지름 → 궤도 반지름으로 붙어가는 속도")]
    public float radiusLerpSpeed = 5f;

    [Tooltip("기체 방향을 접선 방향으로 회전시키는 속도")]
    public float rotationLerpSpeed = 10f;

    [Header("게임 시작 시 가장 가까운 궤도 자동 탐색 반경")]
    public float startSearchRadius = 5f;

    [Header("콤보 한 바퀴 각도 (기본 360도)")]
    public float fullOrbitForCombo = 360f;

    // 내부 상태
    private bool isOrbiting = false;    // 행성 주위를 도는 중인지
    private bool isFlying = false;      // 직선 비행 중인지

    private Transform orbitCenter;      // 현재 도는 궤도의 중심(행성 위치)
    private float orbitAngle;           // 궤도 상의 각도 (degree)
    private float orbitDirection = 1f;  // +1 = 반시계, -1 = 시계

    private float currentRadius;        // 현재 실제 반지름 (계속 Lerp로 보간됨)
    private float targetRadius;         // 목표 반지름 (CircleCollider2D에서 가져옴)

    private Vector2 flyDirection;       // 직선 비행 방향

    private Rigidbody2D rb;
    private ParticleSystem particle;
    private PlanetSpawner spawner;

    // 콤보용
    private float orbitAccumulatedAngle = 0f; // 이번 행성에서 누적 회전 각도
    private bool comboBrokenThisOrbit = false;

    // 기본 속도(콤보 전)
    private float baseOrbitSpeed;
    private float baseLaunchSpeed;

    // Player(카메라)에서 참조
    public bool IsOrbiting => isOrbiting;
    public Transform CurrentOrbitCenter => orbitCenter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        particle = GetComponentInChildren<ParticleSystem>();

        rb.isKinematic = true;
        rb.gravityScale = 0f;

        baseOrbitSpeed = orbitSpeed;
        baseLaunchSpeed = launchSpeed;

        spawner = FindAnyObjectByType<PlanetSpawner>();
    }

    private void Start()
    {
        // 시작 위치 주변에서 가장 가까운 Orbit 찾기
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, startSearchRadius);
        Collider2D nearestOrbit = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Orbit"))
                continue;

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
    }

    private void Update()
    {
        // 궤도 도는 중에 마우스 클릭 → 탈출
        if (isOrbiting && Input.GetMouseButtonDown(0))
        {
            LaunchFromOrbit();
        }

        if (isOrbiting)
            UpdateOrbit();
        else if (isFlying)
            UpdateFlight();
    }

    // ─────────────────────────────────────
    // 궤도 회전
    // ─────────────────────────────────────
    private void UpdateOrbit()
    {
        if (orbitCenter == null) return;

        // 1) 궤도 반지름 목표값 계산
        CircleCollider2D circle = orbitCenter.GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            Vector3 scale = orbitCenter.lossyScale;
            float maxScale = Mathf.Max(scale.x, scale.y);
            float radiusFromCollider = circle.radius * maxScale;
            targetRadius = radiusFromCollider;
        }

        // 현재 반지름을 부드럽게 궤도 반지름으로 수렴
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, radiusLerpSpeed * Time.deltaTime);

        float comboMul = (GameManager.Instance != null) ? GameManager.Instance.ScoreMultiple : 1f;
        float prevAngle = orbitAngle;

        // 각도 갱신 (콤보 배율 적용)
        orbitAngle += orbitDirection * baseOrbitSpeed * comboMul * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        Vector2 center = orbitCenter.position;
        Vector2 newPos = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * currentRadius;
        transform.position = newPos;

        // 접선 방향으로 회전
        Vector2 tangent = (orbitDirection > 0)
            ? new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad))   // 반시계
            : new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)); // 시계

        Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, tangent);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);

        // ── 콤보 각도 누적 및 슬라이더 갱신 ──
        float delta = Mathf.DeltaAngle(prevAngle, orbitAngle);
        orbitAccumulatedAngle += Mathf.Abs(delta);

        float remain01 = 1f - (orbitAccumulatedAngle / fullOrbitForCombo);
        if (GameManager.Instance != null)
            GameManager.Instance.SetComboProgress01(remain01);

        // 한 바퀴 이상 돌면 콤보 리셋 (한 번만)
        if (!comboBrokenThisOrbit && orbitAccumulatedAngle >= fullOrbitForCombo)
        {
            comboBrokenThisOrbit = true;
            GameManager.Instance?.ResetCombo();
        }
    }

    // ─────────────────────────────────────
    // 직선 비행
    // ─────────────────────────────────────
    private void UpdateFlight()
    {
        float comboMul = (GameManager.Instance != null) ? GameManager.Instance.SpeedMultiple : 1f;
        Vector2 move = flyDirection * (baseLaunchSpeed * comboMul * Time.deltaTime);
        transform.position += (Vector3)move;

        float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // ─────────────────────────────────────
    // 새 궤도 진입
    // ─────────────────────────────────────
    public void EnterOrbit(Transform orbitTransform)
    {
        if (orbitTransform == null) return;

        orbitCenter = orbitTransform;
        isOrbiting = true;
        isFlying = false;

        spawner?.OnRocketEnterOrbit(orbitTransform);

        // 1) 목표 반지름 계산
        CircleCollider2D circle = orbitCenter.GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            Vector3 scale = orbitCenter.lossyScale;
            float maxScale = Mathf.Max(scale.x, scale.y);
            float radiusFromCollider = circle.radius * maxScale;
            targetRadius = radiusFromCollider;
        }
        else
        {
            targetRadius = Vector2.Distance(transform.position, orbitCenter.position);
        }

        // 2) 현재 위치 기준 반지름/각도 계산
        Vector2 center = orbitCenter.position;
        Vector2 offset = (Vector2)transform.position - center;
        currentRadius = offset.magnitude;

        if (currentRadius < 0.01f)
        {
            currentRadius = targetRadius * 0.5f;
            offset = Vector2.right * currentRadius;
            transform.position = center + offset;
        }

        orbitAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        float rad = orbitAngle * Mathf.Deg2Rad;

        // 3) 현재 바라보는 방향을 기준으로 어느 방향으로 도는 게 자연스러운지 결정
        Vector2 tangentCCW = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad)); // 반시계
        Vector2 tangentCW = new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)); // 시계

        Vector2 forward = transform.up.normalized;
        float dotCCW = Vector2.Dot(forward, tangentCCW);
        float dotCW = Vector2.Dot(forward, tangentCW);
        orbitDirection = (dotCCW >= dotCW) ? 1f : -1f;

        // ── 콤보 관련 초기화 ──
        orbitAccumulatedAngle = 0f;
        comboBrokenThisOrbit = false;
        GameManager.Instance?.SetComboProgress01(1f);

        // 새 행성에 도달했을 때 점수 추가(현재 콤보 배율 적용)
        GameManager.Instance?.AddScore(1);
    }

    // ─────────────────────────────────────
    // 궤도 탈출 (마우스 클릭)
    // ─────────────────────────────────────
    public void LaunchFromOrbit()
    {
        if (!isOrbiting)
            return;

        // 한 바퀴 돌기 전에 나갔다면 콤보 증가
        if (!comboBrokenThisOrbit)
        {
            GameManager.Instance?.IncreaseCombo();
        }

        isOrbiting = false;
        isFlying = true;

        if (particle != null)
            particle.Play();

        flyDirection = transform.up.normalized;
    }

    // 궤도 트리거에 닿았을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Orbit"))
            return;

        // 비행 중일 때만 새 궤도에 캡처
        if (isFlying)
        {
            EnterOrbit(other.transform);
        }
    }
}
