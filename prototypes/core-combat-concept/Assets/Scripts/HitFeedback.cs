// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can fast/responsive melee combat with combo chains in Unity still feel weighty?
// Date: 2026-08-10

using System.Collections;
using UnityEngine;

/// <summary>
/// Central "juice" dispatcher: hit-stop (brief Time.timeScale freeze), camera shake,
/// and a procedurally generated placeholder hit sound. This is the single most important
/// script for the prototype's hypothesis — it's what makes a fast hit feel weighty.
/// </summary>
public class HitFeedback : MonoBehaviour
{
    public static HitFeedback Instance { get; private set; }

    [Header("Hit-stop")]
    public float hitStopDuration = 0.06f;   // real-world seconds the game freezes
    public float finisherHitStopDuration = 0.12f;

    [Header("Camera shake")]
    public SimpleCameraShake cameraShake;
    public float shakeIntensity = 0.15f;
    public float finisherShakeIntensity = 0.35f;
    public float shakeDuration = 0.12f;

    [Header("Sound")]
    public AudioSource audioSource;
    [Range(0f, 1f)] public float baseVolume = 0.6f;

    AudioClip[] _hitTones; // one per combo step, pitched differently
    Coroutine _hitStopRoutine;

    void Awake()
    {
        Instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        _hitTones = new AudioClip[]
        {
            PlaceholderTone.Generate(180f, 0.08f), // step 1 — light thud
            PlaceholderTone.Generate(140f, 0.10f), // step 2 — mid thud
            PlaceholderTone.Generate(90f,  0.16f), // step 3 finisher — heavy thud
        };
    }

    public void PlayHit(int comboStep)
    {
        bool isFinisher = comboStep >= _hitTones.Length - 1;

        if (_hitStopRoutine != null) StopCoroutine(_hitStopRoutine);
        _hitStopRoutine = StartCoroutine(HitStopRoutine(isFinisher ? finisherHitStopDuration : hitStopDuration));

        if (cameraShake != null)
            cameraShake.Shake(isFinisher ? finisherShakeIntensity : shakeIntensity, shakeDuration);

        if (audioSource != null && comboStep >= 0 && comboStep < _hitTones.Length)
            audioSource.PlayOneShot(_hitTones[comboStep], baseVolume);
    }

    IEnumerator HitStopRoutine(float duration)
    {
        float originalScale = Time.timeScale;
        Time.timeScale = 0.02f; // near-freeze, not full stop — full stop reads as a hitch, not weight
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalScale;
    }
}

/// <summary>
/// Generates a short decaying tone in memory so the prototype needs zero audio assets.
/// </summary>
public static class PlaceholderTone
{
    public static AudioClip Generate(float frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        var data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 18f); // fast decay = "thud" not "beep"
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
        }

        var clip = AudioClip.Create($"HitTone_{frequency}", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
