using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public GameObject pausePanel;   // 일시정지 패널

    private bool isPaused = false;


    private TextMeshProUGUI text;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                PauseGame();
        }
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
