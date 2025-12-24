using System.Collections;
using UnityEngine;
using TMPro;
using System.Text;

public class LineByLineText : MonoBehaviour
{
    public TMP_Text textUI;
    public string GameoverText;
    public float lineDelay = 0.5f;

    private void OnEnable()
    {
        StartCoroutine(ShowLines());
    }

    IEnumerator ShowLines()
    {
        textUI.text = "";

        foreach (char line in GameoverText)
        {
            if (line == '#')
            {
                textUI.text += '\n';
                continue;
            }
            textUI.text += line;
            yield return new WaitForSecondsRealtime(lineDelay);
        }

    }
}

