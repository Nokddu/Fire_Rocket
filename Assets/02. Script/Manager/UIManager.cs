using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public GameObject pausePanel;   // 일시정지 패널

    private bool isPaused = false;


    [SerializeField]
    private TextMeshProUGUI text;
    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        GameManager.Instance.SetScore += ScoreSet;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                PauseGame();
        }
    }

    private void ScoreSet(int val)
    {
        text.text = val.ToString();
        Debug.Log(val);
    }

    public void PauseGame()
    {
        if (isPaused)
        {
            // 일시정지 해제
            Time.timeScale = 1f;
            isPaused = false;
        }
        else
        {
            // 일시정지 시작
            Time.timeScale = 0f;
            isPaused = true;
        }

    }


}
