using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    public string genSeed = "Default";
    public int CurrentSeed = 0;

    void OnValidate()
    {
        CurrentSeed = genSeed.GetHashCode();
        Random.InitState(CurrentSeed);
    }
}
