using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private AudioSource[] sfx;

    private void Awake()

    {
        DontDestroyOnLoad(this.gameObject);

        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

    }

    public void PlaySFX(int sfxToPlay)
    {
        if (sfxToPlay < sfx.Length)
        {

            sfx[sfxToPlay].Play();
        }
    }

    //for looping sfx like the glide.  
    public bool IsSFXPlaying(int sfxToPlay)
    {
        return sfx[sfxToPlay].isPlaying;
    }

    public void StopSFX(int sfxStop)
    {
        if (sfxStop < sfx.Length)
        {
            sfx[sfxStop].Stop();
        }
    }


}
