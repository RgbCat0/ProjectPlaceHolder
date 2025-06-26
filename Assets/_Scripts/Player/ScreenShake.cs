using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }
    private Coroutine _shakeCoroutine;
    private CinemachineBasicMultiChannelPerlin _perlinNoise;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _perlinNoise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (_perlinNoise == null)
        {
            Debug.LogError("CinemachineBasicMultiChannelPerlin component not found on the GameObject.");
        }
    }

    public void Shake(float duration, float magnitude, float frequency = 2f)
    {
        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);
        _perlinNoise.FrequencyGain = 0f;
        _perlinNoise.AmplitudeGain = 0f;
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude, frequency));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude, float frequency)
    {
        _perlinNoise.FrequencyGain = frequency;
        while (true) // smoothly transition the shake effect
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                
                _perlinNoise.AmplitudeGain = Mathf.Lerp(0f, magnitude, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _perlinNoise.FrequencyGain = 0f;
            _perlinNoise.AmplitudeGain = 0f;
            yield break; // exit the coroutine
        }
        
    }
}