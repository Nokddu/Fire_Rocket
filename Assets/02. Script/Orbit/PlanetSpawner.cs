using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawner : MonoBehaviour
{
    [Header("행성 이미지 프리팹들")]
    public GameObject[] planetPrefabs;

    [Header("초기 행성 시작 Y")]
    public float startPlanet = 0f;

    [Header("처음 한 번에 생성할 행성 수")]
    public int initialSpawnCount = 6;   // 시작할 때 대략 1개 + 앞쪽 몇 개

    [Header("현재 행성 기준 앞/뒤 유지 개수")]
    public int aheadCount = 5;          // 앞(위쪽)으로 항상 유지할 개수
    public int maxBehindCount = 3;      // 뒤(아래쪽)으로 최대 유지 개수

    [Header("행성 X축 위치 범위")]
    public float minX = -1.0f;
    public float maxX = 1.0f;

    [Header("궤도 간 최소 거리")]
    public float orbitGap = 7f;

    // 행성 + 궤도 한 쌍
    private class PlanetEntry
    {
        public GameObject planetGO;
        public GameObject orbitGO;
    }

    // 현재까지 생성된 행성 목록 (Y가 작은 것부터 순서대로)
    private readonly List<PlanetEntry> planets = new List<PlanetEntry>();

    private float nextY;          // 다음 행성이 생성될 Y
    private float prevOrbitHalf;  // 이전 궤도 반지름 절반
    private int currentIndex = -1; // 플레이어가 도달한 "현재" 행성 인덱스

    private void Start()
    {
        nextY = startPlanet;
        prevOrbitHalf = 0f;

        // 시작할 때 몇 개 먼저 깔아두기
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnNextPlanet();
        }
    }

    /// <summary>
    /// 다음 위치에 행성 + 궤도 한 쌍을 생성하고 리스트에 추가
    /// </summary>
    private void SpawnNextPlanet()
    {
        if (planetPrefabs == null || planetPrefabs.Length == 0)
        {
            Debug.LogWarning("PlanetSpawner: planetPrefabs 비어 있음");
            return;
        }

        GameObject prefabGO = planetPrefabs[Random.Range(0, planetPrefabs.Length)];

        float x = Random.Range(minX, maxX);
        Vector3 position = new Vector3(x, nextY, 0f);

        // 행성 생성
        GameObject planetGO = Instantiate(prefabGO, position, Quaternion.identity);
        Planet planet = planetGO.GetComponent<Planet>();
        if (planet == null)
        {
            Debug.LogError("PlanetSpawner: Planet 컴포넌트가 프리팹에 없습니다.");
        }

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
            orbitGO = Instantiate(planet.orbitPrefab, position, Quaternion.identity);

            float orbitScale = planet.GetOrbitScale(planetScale);
            orbitGO.transform.localScale = Vector3.one * orbitScale;

            orbitHalf = orbitScale * 0.5f;
        }

        // 다음 Y 위치 업데이트 (이전 궤도 반지름 + 이번 궤도 반지름 + 간격)
        nextY += (prevOrbitHalf + orbitHalf) + orbitGap;
        prevOrbitHalf = orbitHalf;

        // 리스트에 저장
        planets.Add(new PlanetEntry
        {
            planetGO = planetGO,
            orbitGO = orbitGO
        });
    }

    /// <summary>
    /// 로켓이 어떤 궤도에 도달했을 때 PlanetSpawner에 알려주는 함수.
    /// orbitTransform = 궤도 오브젝트의 Transform
    /// </summary>
    public void OnRocketEnterOrbit(Transform orbitTransform)
    {
        if (orbitTransform == null) return;

        // 0. 어떤 행성의 궤도인지 찾기
        int index = planets.FindIndex(p => p.orbitGO != null && p.orbitGO.transform == orbitTransform);
        if (index == -1)
        {
            // 리스트에 없는 궤도라면 그냥 무시
            return;
        }

        // 현재 행성 인덱스 갱신
        currentIndex = index;

        // ─────────────────────────────
        // 1) 앞쪽(위쪽) 행성 미리 생성 : 항상 aheadCount 개 유지
        // ─────────────────────────────
        int ahead = planets.Count - 1 - currentIndex;  // 현재 기준 앞으로 몇 개 있는지
        while (ahead < aheadCount)
        {
            SpawnNextPlanet();
            ahead = planets.Count - 1 - currentIndex;
        }

        // ─────────────────────────────
        // 2) 뒤쪽(아래쪽) 행성 정리 : maxBehindCount 개만 남기고 나머지 삭제
        // ─────────────────────────────
        // 인덱스 0 ~ currentIndex-1 이 뒤쪽 행성들
        int behind = currentIndex;
        int removeCount = Mathf.Max(0, behind - maxBehindCount);

        for (int i = 0; i < removeCount; i++)
        {
            var entry = planets[0];

            if (entry.planetGO != null)
                Destroy(entry.planetGO);
            if (entry.orbitGO != null)
                Destroy(entry.orbitGO);

            planets.RemoveAt(0);
            currentIndex--;  // 리스트 앞에서 지웠으니까 인덱스 한 칸씩 당겨짐
        }
    }
}
