using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private int score;
    public int Score { get => score; set { score = value; SetScore.Invoke(value); }  }

    public Action<int> SetScore;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Score++;
        }

    }
}
