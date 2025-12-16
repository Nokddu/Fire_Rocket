using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject pausePanel;   // 일시정지 패널
    public GameObject restart; //재시작 패널
    private bool isPaused = false;

    [Header("점수 텍스트")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("콤보 UI")]
    [SerializeField] private Transform comboSlider;           // min=0, max=1
    [SerializeField] private TextMeshProUGUI comboMultipleText;    // "x1", "x2", "x3"
    [SerializeField] private TextMeshProUGUI comboText;

    private void Start()
    {
        if (scoreText == null)
            scoreText = GetComponentInChildren<TextMeshProUGUI>();

        GameManager.Instance.SetScore += ScoreSet;
        GameManager.Instance.OnComboMultipleChanged += ComboMultipleChanged;
        GameManager.Instance.OnComboProgressChanged += ComboProgressChanged;
        GameManager.Instance.OnComboChanged += ComboChanged; 

        if (comboSlider != null)
            comboSlider.localScale = Vector3.right;

        if (comboMultipleText != null)
            comboMultipleText.text = "X1";
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
            comboSlider.localScale = new Vector3(normalized, 1, 0);
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
