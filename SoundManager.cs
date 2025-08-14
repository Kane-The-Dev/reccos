using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType{
    RUN,
    //JUMP,
    //LAND,
    KICK,
    //GOAL,
    //POWERUP,
    //JETPACK,
    //SMOKE,
    //WHISTLE,
    //BUTTON,
    //WIN,
    //LOSE
}

public class SoundManager : MonoBehaviour
{
    [SerializeField] SoundList[] soundList;
    private static SoundManager instance;
    AudioSource source;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType type, float volume = 1f)
    {
        AudioClip[] clips = instance.soundList[(int)type].sounds;
        int random = UnityEngine.Random.Range(0, clips.Length);

        instance.source.PlayOneShot(clips[random], volume);
    }
}

[Serializable]
public struct SoundList
{
    [SerializeField] string name;
    public AudioClip[] sounds;
}
