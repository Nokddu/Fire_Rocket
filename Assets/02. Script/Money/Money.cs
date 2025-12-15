using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] float range = 0.5f; // 좌우 이동 범위 (0.5면 좌로 0.5, 우로 0.5 = 총 1만큼 움직임)
    [SerializeField] float speed = 5f;   // 왔다갔다 속도


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 재화 획득
        Destroy(gameObject);
    }
}
