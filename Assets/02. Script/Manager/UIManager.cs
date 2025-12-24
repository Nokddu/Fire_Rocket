using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private bool isPaused = false;

    [Header("점수 텍스트")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("콤보 UI")]
    [SerializeField] private Transform comboSlider;           // min=0, max=1
    [SerializeField] private TextMeshProUGUI comboMultipleText;    // "x1", "x2", "x3"
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private CanvasGroup comboGroup;

    [Header("게임 State")]
    [SerializeField] public GameStateController gameStateController;


    // 지울 변수
    [SerializeField] private TextMeshProUGUI bestScore;

    private void Start()
    {
        if (gameStateController == null)
            gameStateController = GetComponent<GameStateController>();

        if (scoreText == null)
            scoreText = GetComponentInChildren<TextMeshProUGUI>();

        GameManager.Instance.SetScore += ScoreSet;
        GameManager.Instance.OnComboMultipleChanged += ComboMultipleChanged;
        GameManager.Instance.OnComboProgressChanged += ComboProgressChanged;
        GameManager.Instance.OnComboChanged += ComboChanged;

        if (comboSlider != null)
            comboSlider.localScale = Vector3.zero;

        if (comboMultipleText != null)
            comboMultipleText.text = "X1";

        bestScore.text = DataManager.LoadData("Score").ToString();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.SetScore -= ScoreSet;
        GameManager.Instance.OnComboMultipleChanged -= ComboMultipleChanged;
        GameManager.Instance.OnComboProgressChanged -= ComboProgressChanged;
        GameManager.Instance.OnComboChanged -= ComboChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                PauseGame();
        }
    }

    private void ScoreSet(int val)
    {
        if (scoreText != null)
            scoreText.text = val.ToString();
    }
    private void ComboMultipleChanged(int combo)
    {
        if (scoreText != null)
            comboMultipleText.text = $"X{combo}";

        switch (combo)
        {
            case 1:
                comboMultipleText.color = Color.white;
                break;
            case 2:
                comboMultipleText.color = Color.yellow;
                break;
            case 3:
                comboMultipleText.color = Color.red;
                break;
        }
    }

    private void ComboChanged(int combo)
    {
        if (comboText != null)
            comboText.text = combo.ToString();
    }

    private void ComboProgressChanged(float normalized)
    {
        if (comboSlider != null)
        {
            comboSlider.localScale = new Vector3(normalized, 1, 0);
            comboGroup.alpha = normalized;
        }
    }

    public void PauseGame() //일시 정지
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
        else
        {
            Time.timeScale = 0f;
            isPaused = true;
        }
    }

    // 게임 다시 시작 (씬 재로드)
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
