using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawner : MonoBehaviour
{
    [Header("행성 이미지들")]
    public GameObject[] planetPrefabs;

    [Header("행성 크기")]
    public float minPlanetScale = 0.7f;
    public float maxPlanetScale = 1.5f;

    [Header("궤도 크기")]
    public GameObject orbitPrefab;
    public float minOrbit = 1.1f;
    public float maxOrbit = 1.5f;

    [Header("행성 생성")]
    public float startPlanet = 0f;
    public float planetCount = 3;   // 생성 개수

    [Header("행성 X축 위치")]
    public float minX = -1.0f;
    public float maxX = 1.0f;

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
            float randomX = Random.Range(minX, maxX);

            GameObject prefab = planetPrefabs[Random.Range(0, planetPrefabs.Length)]; // 행성 프리펩 랜덤 선택

            Vector3 pos = new Vector3(randomX, currentY, 0);
            GameObject planet = Instantiate(prefab, pos, Quaternion.identity);  // 행성 생성

            float scale = Random.Range(minPlanetScale, maxPlanetScale);
            planet.transform.localScale = Vector3.one * scale;          // 행성 크기

            GameObject orbit = Instantiate(orbitPrefab, planet.transform.position, Quaternion.identity);    // 궤도 생성

            float orbitMultiplier = Random.Range(1.05f, 1.25f);
            float orbitScale = scale * orbitMultiplier;
            orbit.transform.localScale = Vector3.one * orbitScale;      // 궤도 크기

            float orbitHalf = orbitScale / 2f;

            currentY += (prevOrbitHalf + orbitHalf) + 7f;   // 궤도 안겹치게 간격

            prevOrbitHalf = orbitHalf;
        }
    }
}
