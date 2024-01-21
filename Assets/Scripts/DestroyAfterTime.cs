using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float timeToDestroy = 2.0f;

    void Awake()
    {
        Destroy(gameObject, timeToDestroy);
    }
}