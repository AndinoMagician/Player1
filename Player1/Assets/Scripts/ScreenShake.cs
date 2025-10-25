using UnityEngine;
using System.Collections;
using Unity.Cinemachine;


public class ScreenShake : MonoBehaviour
{
    private CinemachineCamera virtualCam;
    private CinemachineBasicMultiChannelPerlin noise; 
    private float defaultAmplitude = 0f;

    void Awake()
    {
        virtualCam = GetComponent<CinemachineCamera>();
        if (virtualCam == null)
        {
            Debug.LogError("No Cinemachine");
            return;
        }
        noise = virtualCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogWarning("No CinemachineBasicPerlin");
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

