using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    [Header("¼³Á¤")]
    [SerializeField] int value = 1;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            MoneyManager.Instance.AddMoney(value);
            Destroy(gameObject);
        }
    }
}
