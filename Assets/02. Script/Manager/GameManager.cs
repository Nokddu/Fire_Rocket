using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private int score;
    private int comboCount = 0; // 콤보 수

    [Header("콤보 설정")]
    [SerializeField] private float maxSpeedMultiple = 2.0f;

    // 점수 프로퍼티
    public int Score
    {
        get => score;
        set
        {
            score = value;
            SetScore?.Invoke(value);
        }
    }

    // 기존 점수 이벤트
    public Action<int> SetScore;

    // 콤보 관련 이벤트
    public Action<int> OnComboMultipleChanged;           // 콤보 단계 변경(x1,x2,x3)
    public Action<float> OnComboProgressChanged; // 콤보 슬라이더(0~1)
    public Action<int> OnComboChanged; // 콤보

    //// 현재 콤보 단계와 배율(1,2,3)
    //public int ComboLevel => comboLevel;
    //public float ComboMultiplier => comboLevel;

    public int ComboCount => comboCount;

    // 콤보레벨 = 점수계수 단계 (1,2,3)
    public int ComboLevel
    {
        get
        {
            if (comboCount < 5)
                return 1;
            if (comboCount < 10)
                return 2;
            return 3;
        }
    }

    public float ScoreMultiple => ComboLevel;   // 콤보레벨에 따른 점수배율

    public float SpeedMultiple   // 콤보에 따른 속도 배율
    {
        get
        {
            //if (ComboLevel == 1)
            //    return 1.1f;
            //if (ComboLevel == 2)
            //    return Mathf.Lerp(1.1f, 1.5f, 1f); // 정확히 1.5
            float multiple = 1 + (comboCount * 0.1f);
            return Mathf.Clamp(multiple, 1,2); ; // 3레벨 = 2.0
        }
    }

    /// <summary>
    /// 콤보 배율을 적용해서 점수를 올림
    /// </summary>
    public void AddScore(int baseScore = 1)
    {
        Score += Mathf.RoundToInt(baseScore * ScoreMultiple);
    }

    /// <summary>
    /// 콤보 증가 (최대 maxComboLevel)
    /// </summary>
    public void IncreaseCombo()
    {
        int before = ComboLevel;

        comboCount++;
        OnComboChanged?.Invoke(comboCount);
        if (ComboLevel != before)
        {
            OnComboMultipleChanged?.Invoke(ComboLevel);
        }
    }

    /// <summary>
    /// 콤보 리셋 (x1)
    /// </summary>
    public void ResetCombo()
    {
        int before = ComboLevel;

        comboCount = 0;
        OnComboChanged?.Invoke(comboCount);
        if (ComboLevel != before)
            OnComboMultipleChanged?.Invoke(ComboLevel);
    }

    /// <summary>
    /// 0~1 사이 값으로 콤보 슬라이더 갱신
    /// </summary>
    public void SetComboProgress01(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        OnComboProgressChanged?.Invoke(normalized);
    }
}