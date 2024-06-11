using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveDestroyedController : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        SpawnManager.OnWaveDestroyed += Popup;

        anim = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        SpawnManager.OnWaveDestroyed -= Popup;
    }

    void Popup(int reward)
    {
        //TODO: include reward amount in popup
        anim.SetTrigger("Popup");
    }
}
