using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRange : MonoBehaviour
{
    public AudioSource audioSource;

    void OnTriggerEnter(Collider collider)
    {

        if (collider.tag == "Player" && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

}
