using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public Camera cam;


    private void Start()
    {
        cam = Camera.main;
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && rb.velocity == Vector2.zero)
        {
            StartCoroutine(FireSpaceShip());
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
