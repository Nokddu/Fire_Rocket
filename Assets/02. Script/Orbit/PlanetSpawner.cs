using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawner : MonoBehaviour
{
    [Header("행성 이미지 프리팹들")]
    public GameObject[] planetPrefabs;

    [Header("초기 행성 시작 Y")]
    public float startPlanet = 0f;

    [Header("처음 한 번에 생성할 행성 수")]
    public int initialSpawnCount = 3;

    [Header("현재 행성 기준 앞/뒤 유지 개수")]
    public int aheadCount = 5;      // 앞쪽(위쪽)으로 최소 몇 개 유지할지
    public int maxBehindCount = 3;  // 뒤쪽(아래쪽)으로 최대 몇 개 남길지

    [Header("행성 X축 위치 범위")]
    public float minX = -3f;
    public float maxX = 3f;

    [Header("궤도 간 최소 거리")]
    public float orbitGap = 3f;

    [Header("카메라 연동 설정 (CameraController 값과 맞춰주세요)")]
    public float camBaseSize = 5f;       // 카메라 기본 사이즈
    public float camSizeScale = 1.1f;    // 궤도 크기에 곱해지는 배율
    public float camOffsetY = 2.0f;      // Player의 Planet Offset Y값과 동일하게 (2.0)
    public float topMargin = 1.0f;
    //임시 재화 프리팹
    public GameObject Money;

    //게임오버라인
    public GameOverLine gameOverLine;


    // 재화 스폰 위치
    private Vector3 prevTrans;
    private Vector3 curTrans;
    private bool isFirstSpawn = true;

    // 행성 + 궤도 한 세트
    private class PlanetEntry
    {
        public GameObject planet;
        public GameObject orbit;
    }

    private readonly List<PlanetEntry> planets = new List<PlanetEntry>();

    private float nextY;           // 다음 행성이 생성될 Y
    private float prevOrbitHalf;   // 이전 궤도 스케일의 절반
    private int currentIndex = -1; // 로켓이 도달한 “현재 행성” 인덱스

    private void Start()
    {
        nextY = startPlanet;
        prevOrbitHalf = 0f;

        // 처음에 몇 개 미리 깔아두기
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnNextPlanet();
        }
    }

    /// <summary>
    /// 행성 + 궤도 한 세트를 생성해서 리스트에 추가
    /// </summary>
    private void SpawnNextPlanet()
    {
        if (planetPrefabs == null || planetPrefabs.Length == 0)
        {
            Debug.LogWarning("PlanetSpawner : planetPrefabs 비어 있음");
            return;
        }

        GameObject prefabGO = planetPrefabs[Random.Range(0, planetPrefabs.Length)];

        float x = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(x, nextY, 0f);

        // 행성 생성
        GameObject planetGO = Instantiate(prefabGO, pos, Quaternion.identity);
        Planet planet = planetGO.GetComponent<Planet>();

        

        float planetScale = 1f;
        if (planet != null)
        {
            planetScale = planet.GetRandomPlanetScale();
            planetGO.transform.localScale = Vector3.one * planetScale;
        }

        // 궤도 생성
        GameObject orbitGO = null;
        float orbitHalf = 0f;

        if (planet != null && planet.orbitPrefab != null)
        {
            orbitGO = Instantiate(planet.orbitPrefab, pos, Quaternion.identity);

            orbitGO.transform.localScale = Vector3.one * planetScale;

            orbitHalf = planetScale * 0.5f;
        }

        SpawnMoney(orbitGO);

        float predictedCamSize = Mathf.Max(camBaseSize, prevOrbitHalf * camSizeScale);

        float visualDistance = camOffsetY + predictedCamSize - topMargin;

        float physicalDistance = prevOrbitHalf + orbitHalf + orbitGap;

        float finalStepY = Mathf.Max(visualDistance, physicalDistance);

        // 다음 Y 위치 갱신 (궤도끼리 안 겹치도록)
        nextY += finalStepY;

        prevOrbitHalf = orbitHalf;

        planets.Add(new PlanetEntry
        {
            planet = planetGO,
            orbit = orbitGO
        });
    }

    // ─────────────────────────────────────
    // 1) 로켓이 실제로 궤도에 “도달했을 때” 호출
    //    → 앞 5 / 뒤 3 유지
    // ─────────────────────────────────────
    public void OnRocketEnterOrbit(Transform orbitTransform)
    {
        if (orbitTransform == null) return;

        int index = planets.FindIndex(p => p.orbit != null && p.orbit.transform == orbitTransform);
        if (index == -1) return;

        gameOverLine.MoveCenter(planets[index].planet.transform);

        currentIndex = index;

        // (1) 현재 기준으로 앞쪽 최소 aheadCount 개 유지
        EnsureAheadFromIndex(index);

        // (2) 뒤쪽은 maxBehindCount 개만 남기고 나머지 삭제
        int behind = currentIndex;                           // 0 ~ currentIndex-1 이 뒤쪽
        int removeCount = Mathf.Max(0, behind - maxBehindCount);

        for (int i = 0; i < removeCount; i++)
        {
            var entry = planets[0];

            if (entry.planet != null) Destroy(entry.planet);
            if (entry.orbit != null) Destroy(entry.orbit);

            planets.RemoveAt(0);
            currentIndex--;  // 앞에서 하나 지울 때마다 인덱스 앞으로 당겨짐
        }
    }

    // ─────────────────────────────────────
    // 2) 플레이어 앞 레이가 궤도에 맞았을 때 호출
    //    → “그 궤도 기준으로 앞에 부족하면 미리 생성”
    //    (currentIndex는 건드리지 않음)
    // ─────────────────────────────────────
    public void OnOrbitRayHit(Transform orbitTransform)
    {
        if (orbitTransform == null) return;

        int index = planets.FindIndex(p => p.orbit != null && p.orbit.transform == orbitTransform);
        if (index == -1) return;

        // 이 궤도 기준으로 앞에 항상 aheadCount 개가 있도록 보장
        EnsureAheadFromIndex(index);
    }

    /// <summary>
    /// 특정 인덱스를 기준으로 앞쪽 행성이 최소 aheadCount 개가 되도록 보장
    /// </summary>
    private void EnsureAheadFromIndex(int index)
    {
        int ahead = planets.Count - 1 - index;   // index 기준 앞으로 몇 개 있는지

        while (ahead < aheadCount)
        {
            SpawnNextPlanet();
            ahead = planets.Count - 1 - index;
        }
    }

    // 재화 스폰 로직
    //private void SpawnMoney(GameObject orbit)
    //{
    //    OrbitCircle circle= orbit.GetComponent<OrbitCircle>();
    //    Transform top = circle.Top;
    //    Transform bottom = circle.Bottom;

    //    curTrans = bottom.position;

    //    if (isFirstSpawn)
    //    {
    //        prevTrans = top.position;
    //        isFirstSpawn = false;
    //        return;
    //    }

    //    Vector3 startPos = prevTrans;
    //    Vector3 endPos = curTrans;
    //    Vector3 direction = (endPos - startPos).normalized;

    //    float t = Random.Range(0.2f, 0.8f);

    //    Vector3 centerPoint = Vector3.Lerp(startPos, endPos, t);

    //    float widthRatio = Mathf.Abs(t - 0.5f) * 2f;

    //    float maxSpread = orbit.transform.localScale.x * 0.5f;
    //    float currentSpreadWidth = widthRatio * maxSpread;

    //    Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

    //    float randomOffset = Random.Range(-currentSpreadWidth, currentSpreadWidth);
    //    Vector3 finalSpawnpos= centerPoint + (perpendicular * randomOffset);

    //    // 로직 바뀔수 있음
    //    // 랜덤 스폰, 일정 행성 갯수
    //    int spawnRate = Random.Range(0, 100);

    //    if (spawnRate >= 0)
    //    {
    //        Instantiate(Money, finalSpawnpos, Quaternion.identity);
    //    }

    //    prevTrans = top.position;
    //}

    private void SpawnMoney(GameObject orbit)
    {
        OrbitCircle circle = orbit.GetComponent<OrbitCircle>();
        Transform top = circle.Top;
        Transform bottom = circle.Bottom;

        // 현재 행성의 진입점 (아래쪽)
        curTrans = bottom.position;

        // 1. 첫 번째 행성이면 이전 위치가 없으니 스킵 (위치만 저장)
        if (isFirstSpawn)
        {
            prevTrans = top.position;
            isFirstSpawn = false;
            return;
        }


        float spreadFactor = 0.5f;

        float minT = 0.25f;
        float maxT = 0.75f;

        // ─────────────────────────────────────────────────────────────
        // 직선 경로를 위한 완벽한 모래시계(X) 공식 적용
        // ─────────────────────────────────────────────────────────────

        // 이전 행성 반지름 (A) vs 현재 행성 반지름 (B)
        float radiusA = prevOrbitHalf;
        float radiusB = orbit.transform.localScale.x * 0.5f;

        // 두 행성 사이의 거리 벡터
        Vector3 startPos = prevTrans; // 이전 행성 Top
        Vector3 endPos = curTrans;    // 현재 행성 Bottom
        Vector3 direction = (endPos - startPos).normalized;

        // 2. 교차점(Cross Point) 비율 계산
        // 반지름이 큰 쪽에서 더 멀리, 작은 쪽에서 더 가까운 지점에서 교차함
        // 식: t_cross = Ra / (Ra + Rb)
        float t_cross = radiusA / (radiusA + radiusB);

        // 3. 랜덤한 위치(t) 선정 (0.2 ~ 0.8 안전 구역)
        float t = Random.Range(minT, maxT);

        // 4. 해당 위치(t)에서 가능한 최대 폭(Width) 계산 (삼각 비례식)
        float currentMaxSpread = 0f;

        if (t <= t_cross)
        {
            // 교차점 이전 (A 행성 쪽 삼각형): A반지름에서 0으로 줄어듦
            // 비례식: width = Ra * (1 - t / t_cross)
            currentMaxSpread = radiusA * (1f - (t / t_cross));
        }
        else
        {
            // 교차점 이후 (B 행성 쪽 삼각형): 0에서 B반지름으로 늘어남
            // 비례식: width = Rb * ((t - t_cross) / (1 - t_cross))
            currentMaxSpread = radiusB * ((t - t_cross) / (1f - t_cross));
        }

        float actualSpread = currentMaxSpread * spreadFactor;

        // 5. 중심 축 위치 구하기
        Vector3 centerPoint = Vector3.Lerp(startPos, endPos, t);

        // 6. 진행 방향의 수직 벡터 (오른쪽/왼쪽)
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

        // 7. 최종 위치: 중심선에서 계산된 폭 안쪽으로 랜덤하게 배치
        float randomOffset = Random.Range(-actualSpread, actualSpread);
        Vector3 finalSpawnPos = centerPoint + (perpendicular * randomOffset);

        // ─────────────────────────────────────────────────────────────
        // 생성 (확률 적용)
        // ─────────────────────────────────────────────────────────────
        int spawnRate = Random.Range(0, 100);
        if (spawnRate < 90) // 90% 확률 (원하는 대로 조절)
        {
            Instantiate(Money, finalSpawnPos, Quaternion.identity);
        }

        // 다음을 위해 현재 Top 저장
        prevTrans = top.position;
    }
}
