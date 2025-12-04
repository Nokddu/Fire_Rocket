using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera cam;
    private ParticleSystem particle;

    private void Start()
    {
        cam = Camera.main;
        rb = gameObject.GetComponent<Rigidbody2D>();
        particle = gameObject.GetComponentInChildren<ParticleSystem>();
    }


    private void Update()
    {
        // 로직 수정 필요 rb.velocity < 백터 제로면 행성 돌때 문제생김.
        if (Input.GetMouseButtonDown(0) && rb.velocity == Vector2.zero)
        {
            StartCoroutine(FireSpaceShip());
            particle.Play();
        }
    }

    private IEnumerator FireSpaceShip()
    {
        float a = 2;
        float b = a / a;
        while (a > 0)
        {
            float force = Mathf.Lerp(1,2,b);
            Vector2 forceVector = transform.up * force;
            rb.AddForce(forceVector, ForceMode2D.Force);
            a -= Time.deltaTime;
            b -= Time.deltaTime;
            yield return null;
        }
    }

    private void LateUpdate()
    {
        cam.transform.position = new Vector3(transform.position.x,transform.position.y + 1.5f, -10);
    }
}
