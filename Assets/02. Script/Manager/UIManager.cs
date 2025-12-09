using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public GameObject pausePanel;   // 일시정지 패널

    private bool isPaused = false;

    private bool isCombo = true;

    // 옮길예정
    int combo;

    float timecheck = 3.6f;


    private TextMeshProUGUI text;

    private void Start()
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

        StopCoroutine(Combo());
        StartCoroutine(Combo());
    }

    IEnumerator Combo()
    {
        combo++;
        while (isCombo)
        {
            timecheck -= Time.deltaTime;
            if (timecheck < 0)
            {
                isCombo = false;
            }
            yield return null;
        }
        combo = 0;
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
