using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private TextMeshProUGUI text;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
}
