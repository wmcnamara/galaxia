using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuObject;
    [SerializeField] private TextMeshProUGUI sensitivityText;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private float minSensitivity = 0.0f;
    [SerializeField] private float maxSensitivity = 3.0f;

    [SerializeField] private Button quitButton;

    private float currentSensitivity;

    public void TogglePauseMenu()
    {
        bool pauseMenuActive = !pauseMenuObject.activeSelf;
        pauseMenuObject.SetActive(pauseMenuActive);

        Cursor.visible = pauseMenuActive;
        Cursor.lockState = pauseMenuActive ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void Start()
    {
        LoadConstantValues(ref currentSensitivity);

        quitButton.onClick.AddListener(QuitGame);

        sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
        sensitivitySlider.value = currentSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;
        sensitivitySlider.minValue = minSensitivity;
        sensitivityText.text = "Sensitivity: " + currentSensitivity;
    }

    private void OnDestroy()
    {
        sensitivitySlider.onValueChanged.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void LoadConstantValues(ref float sensitivity)
    {
        sensitivity = PlayerConfigConsts.Sensitivity;
    }

    private void OnSensitivitySliderChanged(float newSensitivity)
    {
        //Update the player config consts with the new sensitivity. This will save it to the disk
        currentSensitivity = newSensitivity;
        PlayerConfigConsts.Sensitivity = currentSensitivity;
        FindObjectOfType<Player>().ReloadSensitivity();
        sensitivityText.text = "Sensitivity: " + Math.Round(currentSensitivity, 2);
    }
}
