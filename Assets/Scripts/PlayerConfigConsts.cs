using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConfigConsts : MonoBehaviour
{
    public static float Sensitivity
    {
        get
        {
            if (!sensitivityIsLoaded)
            {
                LoadSensitivity();
            }

            return sensitivity;
        }

        set
        {
            sensitivity = value;

            PlayerPrefs.SetFloat(sensitivityPlayerPrefKey, sensitivity);
            PlayerPrefs.Save();
        }
    }

    private static string sensitivityPlayerPrefKey = "PlayerSensitivity";
    private static float sensitivity = 1.0f;
    private static bool sensitivityIsLoaded = false;

    private static void LoadSensitivity()
    {
        sensitivity = PlayerPrefs.GetFloat(sensitivityPlayerPrefKey, 1.0f);
        sensitivityIsLoaded = true;
    }

    private static void LoadAllConsts()
    {
        LoadSensitivity();
    }
}
