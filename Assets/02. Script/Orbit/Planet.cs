using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Header("행성 이미지")]
    public SpriteRenderer spriteRenderer;

    [Header("궤도 프리팹")]
    public GameObject orbitPrefab;

    [Header("행성 크기")]
    public float minPlanetScale = 0.7f;
    public float maxPlanetScale = 1.5f;

    [Header("궤도 크기 배율")]
    public float minOrbit = 1.05f;
    public float maxOrbit = 1.25f;

    public float GetRandomPlanetScale()
    {
        return Random.Range(minPlanetScale, maxPlanetScale);
    }

    public float GetOrbitScale(float planetScale)
    {
        float orbit = Random.Range(minOrbit, maxOrbit);
        return planetScale * orbit;
    }
}
