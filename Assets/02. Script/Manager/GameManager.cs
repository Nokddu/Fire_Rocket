using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private int score;

    [Header("콤보 설정")]
    [SerializeField] private int comboLevel = 1;      // 1 ~ maxComboLevel
    [SerializeField] private int maxComboLevel = 3;   // 최대 3배

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
    public Action<int> OnComboChanged;           // 콤보 단계 변경(x1,x2,x3)
    public Action<float> OnComboProgressChanged; // 콤보 슬라이더(0~1)

    // 현재 콤보 단계와 배율(1,2,3)
    public int ComboLevel => comboLevel;
    public float ComboMultiplier => comboLevel;

    /// <summary>
    /// 콤보 배율을 적용해서 점수를 올림
    /// </summary>
    public void AddScore(int baseScore = 1)
    {
        int add = Mathf.RoundToInt(baseScore * ComboMultiplier);
        Score += add;
    }

    /// <summary>
    /// 콤보 증가 (최대 maxComboLevel)
    /// </summary>
    public void IncreaseCombo()
    {
        int before = comboLevel;
        comboLevel = Mathf.Clamp(comboLevel + 1, 1, maxComboLevel);

        if (comboLevel != before)
        {
            OnComboChanged?.Invoke(comboLevel);
        }
    }

    /// <summary>
    /// 콤보 리셋 (x1)
    /// </summary>
    public void ResetCombo()
    {
        if (comboLevel == 1)
            return;

        comboLevel = 1;
        OnComboChanged?.Invoke(comboLevel);
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
