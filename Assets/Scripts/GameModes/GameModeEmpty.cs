using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeEmpty : GameModeBase
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
}
