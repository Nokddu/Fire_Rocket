using UnityEngine;

[RequireComponent(typeof(RocketOrbitController))]
public class Player : MonoBehaviour
{
    private Camera cam;
    private RocketOrbitController orbitController;

    [Header("카메라 오프셋 (플레이어 기준)")]
    public Vector3 playerOffset = new Vector3(0f, 1.5f, -10f);

    [Header("카메라 오프셋 (행성 기준)")]
    public Vector3 planetOffset = new Vector3(0f, 2.0f, -10f);

    [Header("카메라 위치 스무딩")]
    [Tooltip("카메라가 타깃을 따라갈 때 부드럽게 이동하는 시간")]
    public float smoothTime = 0.15f;

    [Header("카메라 사이즈 설정")]
    [Tooltip("기본 카메라 사이즈 (정지 상태, 작은 궤도일 때)")]
    public float baseSize = 5f;

    [Tooltip("카메라 사이즈 변경 속도")]
    public float sizeLerpSpeed = 3f;

    [Tooltip("궤도 반지름을 카메라 사이즈로 바꿀 때 곱해줄 값")]
    public float orbitSizeScale = 1.1f;   // 1.0이면 궤도 반지름 = 카메라 사이즈

    private Vector3 camVelocity = Vector3.zero;

    private bool isGameOver = false;

    private void Awake()
    {
        orbitController = GetComponent<RocketOrbitController>();
    }

    private void Start()
    {
        GameManager.Instance.player = this;
    }

    public void Init()
    {
        cam = Camera.main;

        GameManager.Instance.OnGameOvered += GameOverCameraSet;

        // 카메라가 정사영(Orthographic)인지 한 번 확인
        if (cam != null && !cam.orthographic)
        {
            cam.orthographic = true;
        }

        if (cam != null)
        {
            cam.orthographicSize = baseSize;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameOvered -= GameOverCameraSet;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isGameOver = true;
        }

        if (isGameOver)
        {
            cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            20f,
            0.5f * Time.deltaTime
        );
        }
    }

    private void GameOverCameraSet()
    {
        isGameOver = true;
    }

    private void LateUpdate()
    {
        if (cam == null || orbitController == null || isGameOver)
            return;

        Vector3 targetPos;
        float targetSize = baseSize;

        // ===========================
        // 1) 궤도 도는 중이면: 행성 기준 + 궤도 크기에 따라 사이즈 조절
        // ===========================
        if (orbitController.IsOrbiting && orbitController.CurrentOrbitCenter != null)
        {
            Transform center = orbitController.CurrentOrbitCenter;
            targetPos = center.position + planetOffset;

            // 궤도 크기 가져오기 (CircleCollider2D 기준)
            float orbitRadius = baseSize; // 기본값: 너무 작으면 그냥 baseSize 기준
            CircleCollider2D circle = center.GetComponent<CircleCollider2D>();
            if (circle != null)
            {
                Vector3 scale = center.lossyScale;
                float maxScale = Mathf.Max(scale.x, scale.y);
                orbitRadius = circle.radius * maxScale;
            }
            else
            {
                // 혹시 콜라이더가 없다면, 플레이어와 행성 사이 거리로 대충 추정
                orbitRadius = Vector2.Distance(transform.position, center.position);
            }

            // 궤도 반지름을 기준으로 카메라 사이즈 계산
            float orbitBasedSize = orbitRadius * orbitSizeScale;

            // 최소값은 baseSize 유지 (너무 작은 궤도는 굳이 더 작게 안 봄)
            targetSize = Mathf.Max(baseSize, orbitBasedSize);
        }
        // ===========================
        // 2) 궤도가 아니면: 플레이어 기준 + 기본 사이즈
        // ===========================
        else
        {
            targetPos = transform.position + playerOffset;
            targetSize = baseSize;
        }

        // 위치 부드럽게 이동
        cam.transform.position = Vector3.SmoothDamp(
            cam.transform.position,
            targetPos,
            ref camVelocity,
            smoothTime
        );

        // 사이즈(Lens) 부드럽게 변경
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetSize,
            sizeLerpSpeed * Time.deltaTime
        );
    }
}
