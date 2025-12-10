using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] float range = 0.5f; // 좌우 이동 범위 (0.5면 좌로 0.5, 우로 0.5 = 총 1만큼 움직임)
    [SerializeField] float speed = 5f;   // 왔다갔다 속도

    private Vector2 startPos; // 기준점 (스폰 위치)

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float xOffset = Mathf.Sin(Time.time * speed) * range;

        transform.position = new Vector2(startPos.x + xOffset, startPos.y);
    }
}
