using UnityEngine;

public class OrbitingObstacleSpawner : MonoBehaviour
{
    [Header("공전 장애물 프리팹")]
    [SerializeField] private OrbitingObstacle obstaclePrefab;

    [Header("스폰")]
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.7f;

    [SerializeField] private float orbitRadiusMultiplier = 1.15f; // 궤도보다 크게
    [SerializeField] private float orbitRadiusAdd = 0f;

    [Header("속도/방향")]
    [SerializeField] private float angularSpeedMin = 70f;
    [SerializeField] private float angularSpeedMax = 140f;
    [SerializeField] private bool randomDirection = true;

    [Header("스폰 금지 각도(선택) - 아래쪽(270도) 근처 피하기")]
    [SerializeField] private float avoidCenterAngle = 270f;
    [SerializeField] private float avoidHalfRange = 35f;
    [SerializeField] private int maxTries = 25;

    [Header("충돌 시 옵션")]
    [SerializeField] private bool restartOnHit = false;

    private OrbitingObstacle spawned;

    private void Start()
    {
        if (obstaclePrefab == null) return;
        if (Random.value > spawnChance) return;

        float angle = PickAngle();
        int dir = randomDirection ? (Random.value < 0.5f ? 1 : -1) : 1;
        float spd = Random.Range(angularSpeedMin, angularSpeedMax);

        spawned = Instantiate(obstaclePrefab, transform.position, Quaternion.identity);
        spawned.Initialize(transform, angle, dir, spd, orbitRadiusMultiplier, orbitRadiusAdd, restartOnHit);
    }

    private float PickAngle()
    {
        for (int i = 0; i < maxTries; i++)
        {
            float a = Random.Range(0f, 360f);
            if (Mathf.Abs(Mathf.DeltaAngle(a, avoidCenterAngle)) > avoidHalfRange)
                return a;
        }
        return (avoidCenterAngle + avoidHalfRange + 10f) % 360f;
    }

    private void OnDestroy()
    {
        if (spawned != null)
            Destroy(spawned.gameObject);
    }
}
