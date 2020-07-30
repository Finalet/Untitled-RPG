// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DemoMusic : MonoBehaviour
{
    [SerializeField] float _stopTime = 175f;

    public static DemoMusic Instance { get; private set; }

    AudioSource _src;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _src = GetComponent<AudioSource>();

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (_src.time > _stopTime)
        {
            _src.Stop();
        }
    }

    public void Play()
    {
        _src.Play();
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutMusic(_src, 2f));
    }

    private static IEnumerator FadeOutMusic(AudioSource a, float duration)
    {
        float startVolume = a.volume;

        while (a.volume > 0f)
        {
            a.volume -= startVolume * Time.deltaTime / duration;
            yield return new WaitForEndOfFrame();
        }

        Destroy(a.gameObject);
    }
}
