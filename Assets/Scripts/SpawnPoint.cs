using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    void Start()
    {
        Destroy(GetComponentInChildren<MeshRenderer>());
    }
}
