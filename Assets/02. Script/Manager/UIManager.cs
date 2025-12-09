using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private TextMeshProUGUI text;
    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        GameManager.Instance.SetScore += ScoreSet;
    }

    private void ScoreSet(int val)
    {
        text.text = val.ToString();
        Debug.Log(val);
    }
}
