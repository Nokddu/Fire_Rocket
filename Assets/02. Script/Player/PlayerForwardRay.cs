using UnityEngine;

public class PlayerForwardRay : MonoBehaviour
{
    [Header("앞으로 쏠 레이 길이")]
    public float rayDistance = 30f;

    [Header("레이가 맞출 레이어 (궤도)")]
    public LayerMask rayLayerMask = ~0;   // 기본값: 전부

    private PlanetSpawner spawner;

    private void Awake()
    {
        spawner = FindObjectOfType<PlanetSpawner>();
    }

    private void Update()
    {
        if (spawner == null) return;

        // 행성에서 출발해서 날아가는 타이밍에만 쓰고 싶으면
        // 부스터 누를 때만 레이 쏘게 조건을 건다.
        // (항상 쏘게 하고 싶으면 if문 지워도 됨)
        if (!Input.GetMouseButton(0))   // 왼쪽 마우스를 부스터로 쓰고 있다면
            return;

        Vector2 origin = transform.position;
        Vector2 dir = transform.up;  // 로켓이 바라보는 방향

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayDistance, rayLayerMask);

        if (hit.collider != null && hit.collider.CompareTag("Orbit"))
        {
            // 이 궤도 기준으로 앞에 부족하면 미리 생성
            spawner.OnOrbitRayHit(hit.collider.transform);
        }
    }

    // 씬에서 디버깅용으로 레이 보이게
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        Vector3 dir = transform.up * rayDistance;

        Gizmos.DrawLine(origin, origin + dir);
    }
}
