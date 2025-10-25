using UnityEngine;
using System.Collections;
using Unity.Cinemachine;


public class ScreenShake : MonoBehaviour
{
    private CinemachineCamera virtualCam;
    private CinemachineBasicMultiChannelPerlin noise;   // The new Perlin noise component in 3.x
    private float defaultAmplitude = 0f;

    void Awake()
    {
        virtualCam = GetComponent<CinemachineCamera>();
        if (virtualCam == null)
        {
            Debug.LogError("❌ No CinemachineCamera component found on this GameObject.");
            return;
        }

        // ✅ Use GetComponent to fetch the BasicPerlin noise instead of TryGetComponentOfType
        noise = virtualCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogWarning("⚠️ No CinemachineBasicPerlin found. Add a 'Basic Perlin' Noise extension in the Inspector.");
        }
        else
        {
            defaultAmplitude = noise.AmplitudeGain;
        }
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        if (noise == null)
            yield break;

        noise.AmplitudeGain = magnitude;
        yield return new WaitForSeconds(duration);
        noise.AmplitudeGain = defaultAmplitude;
    }
}

