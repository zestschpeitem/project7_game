using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField]
    private List<Datasound> dataSounds = new List<Datasound>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySoundEffect(string clipName)
    {
        var audioClip = GetAudioClip(clipName);
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void StopSoundEffect(string clipName)
    {
        audioSource.Stop();
    }

    private AudioClip GetAudioClip(string clipName)
    {
        AudioClip audioClip = null;

        foreach (var sound in dataSounds)
        {
            if (sound.name == clipName)
            {
                audioClip = sound.audioClip;
            }
        }

        return audioClip;
    }

    [Serializable]
    private class Datasound
    {
        public string name;
        public AudioClip audioClip;
    }
}

