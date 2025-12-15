using TMPro;
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
    [SerializeField] private TextMeshProUGUI comboText;    // "x1", "x2", "x3"

    private void Start()
    {
        if (scoreText == null)
            scoreText = GetComponentInChildren<TextMeshProUGUI>();

        GameManager.Instance.SetScore += ScoreSet;
        GameManager.Instance.OnComboChanged += ComboChanged;
        GameManager.Instance.OnComboProgressChanged += ComboProgressChanged;

        if (comboSlider != null)
            comboSlider.localScale = Vector3.right;

        if (comboText != null)
            comboText.text = "0";
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.SetScore -= ScoreSet;
        GameManager.Instance.OnComboChanged -= ComboChanged;
        GameManager.Instance.OnComboProgressChanged -= ComboProgressChanged;
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
