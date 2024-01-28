using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI onScreenMessageText;

    private void Awake()
    {
        onScreenMessageText.text = "";
    }

    public void SetLocalOnScreenMessageText(string msg)
    {
        onScreenMessageText.text = msg;

        CancelInvoke();
    }

    public void SetLocalOnScreenMessageText(string msg, float timeToClear)
    {
        onScreenMessageText.text = msg;

        CancelInvoke();
        Invoke(nameof(ClearOnScreenMessageText), timeToClear);
    }

    public void ClearOnScreenMessageText()
    {
        onScreenMessageText.text = "";
    }
}
