using System;
using UnityEngine;

public class GameManager : Singleton<MonoBehaviour>
{
    private int score;
    public int Score { get => score; set { score = value; SetScore.Invoke(value); }  }

    public Action<int> SetScore;

    private void Start()
    {
        SetScore += TestDebug;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Score++;
        }

    }

    public void TestDebug(int val)
    {
        Debug.Log(val);
    }
}
