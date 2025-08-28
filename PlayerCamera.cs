using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    GameManager gm;
    public CinemachineFreeLook cfl;
    //public Vector2 mouseTurn;
    public float sensitivity = 1f;
    public GameObject settingsPanel;
    
    [SerializeField]
    ParticleSystem speedEffect;
    
    void Start()
    {
        gm = GameManager.instance;
        sensitivity = 1f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //mouseTurn.x = Input.GetAxis("Mouse X") * -1f;
        //mouseTurn.y = Input.GetAxis("Mouse Y") * -1f;

        sensitivity = gm.camSensitivity;
        if (gm.inSettings)
        sensitivity = 0;

        cfl.m_XAxis.m_MaxSpeed = 1000f * sensitivity;
        cfl.m_YAxis.m_MaxSpeed = 8f * sensitivity;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            settingsPanel.SetActive(true);
            gm.inSettings = true;
        }
    }

    public void FollowPlayer(Transform x)
    {
        cfl.Follow = x;
        cfl.LookAt = x;
    }

    //screenshake
    public void Shake(float intensity, float duration)
    {
        if(gm.shakeEnabled)
            StartCoroutine(ScreenShake(intensity, duration));
    }

    IEnumerator ScreenShake(float shakeIntensity, float shakeDuration)
    {
        Noise(shakeIntensity * 2f, shakeIntensity);
        yield return new WaitForSeconds(shakeDuration);
        Noise(0, 0);
    }

    void Noise(float amplitudeGain, float frequencyGain)
    {
        for(int i = 0; i < 3; i++)
        {
            cfl.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = amplitudeGain;
            cfl.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = frequencyGain; 
        } 
    }

    //speeding effect
    public void SpeedUp(float speed)
    {   
        //Debug.Log(speedEffect.isPlaying);
        if (speed > 8.1f)
        {
            var emission = speedEffect.emission;
            emission.rateOverTime = 20f + 15f * (speed - 8f);
            var main = speedEffect.main;
            main.startSpeed = 4f + 2.5f * (speed - 8f);
        }
        else 
        {
            var emission = speedEffect.emission;
            emission.rateOverTime = 0f;
        }
    }
}
