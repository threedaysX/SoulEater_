using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cinemachine used only.
/// </summary>
public class CameraControl : MonoBehaviour
{
    private static CinemachineBrain brain;
    private static CinemachineClearShot clearShot;

    private void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        clearShot = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineClearShot>();
    }

    public static CinemachineVirtualCamera GetCurrentActiveCamera()
    {    
        if (clearShot != null)
        {
            // GetCurrentCamera - Active
            return clearShot.LiveChild as CinemachineVirtualCamera;
        }
        return null;
    }

    public static void OnDisableCall()
    {
        Zoom.Instance.OnDisableCall();
        Follow.Instance.OnDisableCall();
        Shake.Instance.OnDisableCall();
    }

    #region Zoom Camera
    public class Zoom : Singleton<Zoom> 
    {
        private List<ZoomInSetting> zoomInSettings = new List<ZoomInSetting>();
        private int currentIndex = 0;
        private float nextStartTime = 0;
        private bool resetTrigger = true;

        #region Private
        private void Update()
        {
            CheckToZoomCamera();
        }

        private void CheckToZoomCamera()
        {
            if (zoomInSettings.Count > 0)
            {
                // Reset First Time Zoom Delay.
                if (resetTrigger)
                {
                    nextStartTime = Time.time + zoomInSettings[currentIndex].startDelay;
                    resetTrigger = false;
                }
                else if (!resetTrigger && Time.time >= nextStartTime)
                {
                    StartCoroutine(ZoomInCamera(zoomInSettings[currentIndex].finalZoomSize, zoomInSettings[currentIndex].duration));
                    nextStartTime = Time.time + zoomInSettings[currentIndex].duration + GetZoomSettingNextStartDelay();
                    currentIndex++;
                }

                // When finished, Clear zoomInSettings and reset state.
                if (currentIndex > zoomInSettings.Count - 1)
                {
                    ResetSettings();
                }
            }
        }

        private void ResetSettings()
        {
            resetTrigger = true;
            currentIndex = 0;
            zoomInSettings.Clear();
        }

        private IEnumerator ZoomInCamera(float finalZoomSize, float duration)
        {
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            CinemachineClearShot currentVcamParent = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineClearShot>();
            if (currentVcamParent != null)
            {
                CinemachineVirtualCamera currentVcam = currentVcamParent.LiveChild as CinemachineVirtualCamera;
                if (currentVcam != null)
                {
                    float originZoomSize = currentVcam.m_Lens.OrthographicSize;
                    float timeLeft = duration;
                    while (timeLeft > 0)
                    {
                        currentVcam.m_Lens.OrthographicSize += (finalZoomSize - originZoomSize) * Time.deltaTime / duration;
                        timeLeft -= Time.deltaTime;
                        yield return null;
                    }
                    if (currentVcam.m_Lens.OrthographicSize != finalZoomSize)
                    {
                        currentVcam.m_Lens.OrthographicSize = finalZoomSize;
                    }
                }
            }
        }

        private float GetZoomSettingNextStartDelay()
        {
            if (currentIndex + 1 >= zoomInSettings.Count)
                return 0f;

            return zoomInSettings[currentIndex + 1].startDelay;
        }
        #endregion

        #region Public
        public void AddSet(IEnumerable<ZoomInSetting> zoomInSettings)
        {
            this.zoomInSettings.AddRange(zoomInSettings);
        }

        public void OnDisableCall()
        {
            StopAllCoroutines();
        }
        #endregion
    }
    #endregion

    #region Follow Target
    public class Follow : Singleton<Follow>
    {
        private List<CameraFollowSetting> settings = new List<CameraFollowSetting>();
        private int currentIndex = 0;
        private bool goNextTrigger = true;

        #region Private
        private void Update()
        {
            CheckToFollow();
        }

        private void CheckToFollow()
        {
            if (settings.Count > 0)
            {
                if (goNextTrigger)
                {
                    goNextTrigger = false;
                    // *Note: When Coroutine end, set [goNextTrigger] true. 
                    StartCoroutine(FollowTarget(settings[currentIndex++]));
                }

                // When finished, clear settings and reset index.
                if (currentIndex > settings.Count - 1)
                {
                    ResetSettings();
                }
            }
        }

        private void ResetSettings()
        {
            settings.Clear();
            currentIndex = 0;
        }

        private IEnumerator FollowTarget(CameraFollowSetting setting)
        {
            if (clearShot == null)
                yield break;

            // Set up.
            clearShot.m_ActivateAfter = setting.startDelay;
            clearShot.m_DefaultBlend.m_Time = setting.moveToTargetDuration;
            clearShot.m_DefaultBlend.m_Style = setting.blendType;
            CinemachineVirtualCamera vcam = GetCurrentActiveCamera();
            setting.targetVcam.Priority = vcam.Priority + 1;

            #region Start delay, wait for seconds.
            if (setting.ignoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(setting.startDelay);
            }
            else
            {
                yield return new WaitForSeconds(setting.startDelay);
            }
            #endregion

            // After start delay event.
            if (setting.followStartEvent != default)
            {
                setting.followStartEvent.Invoke();
            }

            #region Move duration and follow on duration, wait for seconds.
            float remainDuration = setting.moveToTargetDuration + setting.duration;
            if (setting.ignoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(remainDuration);
            }
            else
            {
                yield return new WaitForSeconds(remainDuration);
            }
            #endregion

            setting.targetVcam.Priority = setting.originPriority;

            // Follow finished event.
            if (setting.followEndEvent != default)
            {
                setting.followEndEvent.Invoke();
            }

            // Go next camera follow setting.
            goNextTrigger = true;
        }
        #endregion

        #region Public
        public void FollowTargetSimple(Transform target)
        {
            CinemachineVirtualCamera vcam = GetCurrentActiveCamera();
            vcam.Follow = target;
        }

        public IEnumerator FollowTargetSimple(Transform target, float startDelay, float resetDuration, bool ignoreTimeScale = false)
        {
            CinemachineVirtualCamera vcam = GetCurrentActiveCamera();
            Transform originTarget = vcam.Follow;
            if (ignoreTimeScale)
                yield return new WaitForSecondsRealtime(startDelay);
            else
                yield return new WaitForSeconds(startDelay);
            vcam.Follow = target;

            if (ignoreTimeScale)
                yield return new WaitForSecondsRealtime(resetDuration);
            else
                yield return new WaitForSeconds(resetDuration);
            vcam.Follow = originTarget;
        }

        public void AddSet(IEnumerable<CameraFollowSetting> settings)
        {
            this.settings.AddRange(settings);
        }

        public void OnDisableCall()
        {
            ResetSettings();
            StopAllCoroutines();
        }
        #endregion
    }
    #endregion

    #region Shake
    public class Shake : Singleton<Shake>
    {
        private static bool shaking;

        // Cinemachine Shake (Noise => Basic Multi Channel Perlin => Use 6D Shake or custom shake with new profile.)
        private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

        #region public.
        /// <summary>
        /// Shake camera from small to strong power
        /// </summary>
        public void ShakeCameraLinear(float shakeAmplitude, float shakeFrequency, float duration, bool ignoreTimeScale = false, float startDelay = 0f)
        {
            StartCoroutine(ShakeCameraLinearCoroutine(shakeAmplitude, shakeFrequency, duration, ignoreTimeScale, startDelay));
        }

        public void ShakeCamera(float shakeAmplitude, float shakeFrequency, float duration, bool ignoreTimeScale = false, float startDelay = 0f, bool overrideShake = true)
        {
            StartCoroutine(ShakeCameraCoroutine(shakeAmplitude, shakeFrequency, duration, ignoreTimeScale, startDelay, overrideShake));
        }

        public IEnumerator ShakeCameraCoroutine(float shakeAmplitude, float shakeFrequency, float duration, bool ignoreTimeScale, float startDelay, bool overrideShake)
        {
            if (ignoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(startDelay);
            }
            else
            {
                yield return new WaitForSeconds(startDelay);
            }

            // Get current Vcam's noise setting. (Check 6D Shake is added)
            virtualCameraNoise = GetCurrentActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            // If null or in shaking, return.
            if (virtualCameraNoise == null || (shaking && !overrideShake))
                yield break;

            shaking = true;
            float timeleft = duration;
            while (timeleft > 0)
            {
                // Set Cinemachine Camera Noise parameters
                virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
                virtualCameraNoise.m_FrequencyGain = shakeFrequency;

                if (ignoreTimeScale)
                {
                    timeleft -= Time.unscaledDeltaTime;
                }
                else
                {
                    timeleft -= Time.deltaTime;
                }
                
                yield return null;
            }
            ResetShake();
        }

        public IEnumerator ShakeCameraLinearCoroutine(float shakeAmplitude, float shakeFrequency, float duration, bool ignoreTimeScale, float startDelay)
        {
            if (ignoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(startDelay);
            }
            else
            {
                yield return new WaitForSeconds(startDelay);
            }

            // Get current Vcam's noise setting. (Check 6D Shake is added)
            virtualCameraNoise = GetCurrentActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            // If null or in shaking, return.
            if (virtualCameraNoise == null)
                yield break;

            ResetShake();
            shaking = true;
            float ampStep = shakeAmplitude - virtualCameraNoise.m_AmplitudeGain;
            float freqStep = shakeFrequency - virtualCameraNoise.m_FrequencyGain;
            float timeleft = duration;
            while (timeleft > 0)
            {
                float trueDeltaTime = GetDeltaTime(ignoreTimeScale);
                // Set Cinemachine Camera Noise parameters
                if (timeleft > trueDeltaTime)
                {
                    virtualCameraNoise.m_AmplitudeGain += (ampStep * trueDeltaTime / duration);
                    virtualCameraNoise.m_FrequencyGain += (freqStep * trueDeltaTime / duration);

                }
                else
                {
                    virtualCameraNoise.m_AmplitudeGain += (ampStep * timeleft / duration);
                    virtualCameraNoise.m_FrequencyGain += (freqStep * timeleft / duration);
                }
                timeleft -= trueDeltaTime;
                yield return null;
            }
            ResetShake();
        }

        public void OnDisableCall()
        {
            ResetShake();
            StopAllCoroutines();
        }
        #endregion

        private void ResetShake()
        {
            if (virtualCameraNoise != null)
            {
                virtualCameraNoise.m_AmplitudeGain = 0;
                virtualCameraNoise.m_FrequencyGain = 0;
            }
            shaking = false;
        }

        private float GetDeltaTime(bool isUnScaled)
        {
            if (isUnScaled)
            {
                return Time.unscaledDeltaTime;
            }
            else
            {
                return Time.deltaTime;
            }
        }
    }
    #endregion
}

public struct ZoomInSetting
{
    public float finalZoomSize;
    public float duration;
    public float startDelay;
    public bool isTrigger;
}

public struct CameraFollowSetting
{
    public CinemachineVirtualCamera targetVcam;
    public int originPriority;
    public CinemachineBlendDefinition.Style blendType;
    public float startDelay;
    /// <summary>
    /// When camera follow new target, need X seconds to lock on target position.
    /// </summary>
    public float moveToTargetDuration;
    public float duration;
    public bool ignoreTimeScale;
    public Action followStartEvent;
    public Action followEndEvent;

    public CameraFollowSetting(CinemachineVirtualCamera target, CinemachineBlendDefinition.Style type, float duration, float moveToTargetDuration, float startDelay = 0, bool ignoreTimeScale = false, Action followStartEvent = default, Action followEndEvent = default)
    {
        this.targetVcam = target;
        this.originPriority = target.Priority;
        this.blendType = type;
        this.startDelay = startDelay;
        this.moveToTargetDuration = moveToTargetDuration;
        this.duration = duration;
        this.ignoreTimeScale = ignoreTimeScale;
        this.followStartEvent = followStartEvent;
        this.followEndEvent = followEndEvent;
    }
}
