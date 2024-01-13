using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Image crosshair;

    public Image Crosshair
    {
        get
        {
            return crosshair;
        }

        private set
        {
            crosshair = value;
        }
    }
}
