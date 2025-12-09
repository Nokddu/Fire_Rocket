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

            float orbitScale = planet.GetOrbitScale(planetScale);
            orbitGO.transform.localScale = Vector3.one * orbitScale;

            orbitHalf = orbitScale * 0.5f;
        }

        // 다음 Y 위치 갱신 (궤도끼리 안 겹치도록)
        nextY += (prevOrbitHalf + orbitHalf) + orbitGap;
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
}
