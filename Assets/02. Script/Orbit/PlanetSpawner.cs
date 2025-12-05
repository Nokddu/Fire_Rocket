using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawner : MonoBehaviour
{
    [Header("행성 이미지 프리팹들")]
    public GameObject[] planetPrefabs;

    [Header("행성 생성")]
    public float startPlanet = 0f;
    public float planetCount = 3;   // 생성 개수

    [Header("행성 X축 위치")]
    public float minX = -1.0f;
    public float maxX = 1.0f;

    [Header("궤도 간 최소 거리")]
    public float orbitGap = 7f;

    private void Start()
    {
        SpawnPlanets();
    }

    public void SpawnPlanets()
    {
        float currentY = startPlanet;
        float prevOrbitHalf = 0f;   // 이전 궤도 반지름

        for (int i = 0; i < planetCount; i++)
        {
            GameObject prefabGO = planetPrefabs[Random.Range(0, planetPrefabs.Length)]; // 행성 프리펩 랜덤 뽑기

            float x = Random.Range(minX, maxX);
            Vector3 position = new Vector3 (x, currentY, 0f);

            GameObject planetGO = Instantiate(prefabGO,position, Quaternion.identity);
            Planet planet = planetGO.GetComponent<Planet>();

            float scale = planet.GetRandomPlanetScale();
            planet.transform.localScale = Vector3.one * scale;          // 행성 크기

            GameObject orbit = Instantiate(planet.orbitPrefab,position, Quaternion.identity);    // 궤도 생성

            float orbitScale = planet.GetOrbitScale(scale);
            orbit.transform.localScale = Vector3.one * orbitScale;      // 궤도 크기

            float orbitHalf = orbitScale / 2f;

            currentY += (prevOrbitHalf + orbitHalf) + orbitGap;   // 궤도 안겹치게 간격

            prevOrbitHalf = orbitHalf;
        }

        // 처음 행성 위치와 처음 생성 행성 위치 떨어뜨려 놓기
        // 마지막 해성 궤도에 들어왔을때 3개의 행성 생성
        // 행성 클래스를 통한 정리
        // 궤도 회전
        // 궤도에 로켓 트리거를 통해서 충돌반응
    }
}
