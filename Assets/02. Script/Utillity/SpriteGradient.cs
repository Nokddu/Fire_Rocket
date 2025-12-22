using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteGradient : MonoBehaviour
{
    public Color topColor;
    public Color bottomColor;
    private SpriteRenderer sr;
    private Camera cam;
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        cam = Camera.main;

        sr.material.SetColor("_ColorTop", topColor);
        sr.material.SetColor("_ColorBot", bottomColor);
    }

    // Update is called once per frame
    void Update()
    {
        SetScale();

        sr.material.SetColor("_ColorTop", topColor);
        sr.material.SetColor("_ColorBot", bottomColor);
    }

    private void SetScale()
    {
        float worldScreenHeight = cam.orthographicSize * 2f + 1f;

        float worldScreenWidth = worldScreenHeight * cam.aspect + 1f;

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        Vector3 newScale = transform.localScale;
        newScale.x = worldScreenWidth / spriteWidth;
        newScale.y = worldScreenHeight / spriteHeight;

        transform.localScale = newScale;
    }
}
