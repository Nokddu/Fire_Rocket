using System;

public class GameManager : Singleton<GameManager>
{
    private int score = -1;
    public int Score { get => score; set { score = value; SetScore?.Invoke(value); }  }

    public Action<int> SetScore;

    
}
