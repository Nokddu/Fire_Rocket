using System.Collections;
using UnityEngine;
using TMPro;

public class LineByLineText : MonoBehaviour
{
    public TMP_Text textUI;
    public string[] lines;
    public float lineDelay = 0.5f;

    void Start()
    {
        StartCoroutine(ShowLines());
    }

    IEnumerator ShowLines()
    {
        textUI.text = "";

        foreach (string line in lines)
        {
            textUI.text += line + "\n";
            yield return new WaitForSecondsRealtime(lineDelay);
        }
    }
}

