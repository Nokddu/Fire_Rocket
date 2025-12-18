using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverLine : MonoBehaviour
{
    public void MoveCenter(Transform planet)
    {
        if (planet == null) 
            return;

        transform.position= new Vector3 (planet.position.x,planet.position.y + 7,0);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        GameOver();
    }

    public void GameOver()
    {
        GameManager.Instance.GameOver();
        Debug.Log("게임오버");
    }
}
