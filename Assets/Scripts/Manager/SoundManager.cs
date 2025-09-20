using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public List<AudioClip> sounds;
    public static SoundManager instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private void Awake()
    {
        instance = this;
    }

    public void PlaySound(string soundName)
    {
        foreach (var sound in sounds)
        {
            if (sound.name == soundName)
            {
                AudioSource.PlayClipAtPoint(sound, Camera.main.transform.position);
            } 
        }
    }
}
