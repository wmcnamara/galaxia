using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GameModeDataTypePair
{
    public GameModeType GameMode;
    public GameObject GameModeManagerObject;
}

[CreateAssetMenu(menuName = "NewData/AvailableGameModesData")]
public class AvailableGameModesSO : ScriptableObject
{
    public List<GameModeDataTypePair> AvailableGameModes;
}
