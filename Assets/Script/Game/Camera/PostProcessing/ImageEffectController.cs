using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ImageEffectController : Singleton<ImageEffectController>
{
    public Volume volume;

    #region Vignette
    private Vignette _vignette = null;
    private Sequence bleedSequence;
    #endregion
    #region MotionBlur
    private MotionBlur _motionBlur = null;
    #endregion

    private void Start()
    {
        volume.sharedProfile.TryGet(out _vignette);
        volume.sharedProfile.TryGet(out _motionBlur);
    }

    public void BleedVignette(bool start, params VignetteSetting[] settings)
    {
        bleedSequence.Kill(true);
        if (!start)
        {
            _vignette.active = false;
            return;
        }
        bleedSequence = DOTween.Sequence();
        _vignette.active = true;


        foreach (var setting in settings)
        {
            bleedSequence
                .Append(DOTween.To(() => _vignette.color.value, x => _vignette.color.value = x, setting.color, setting.duration))
                .Insert(0f, DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, setting.intensity, setting.duration))
                .Insert(0f, DOTween.To(() => _vignette.smoothness.value, x => _vignette.smoothness.value = x, setting.smoothness, setting.duration));
        }

        bleedSequence.Play();
    }

    public void SetMotionBlur(float intensity, float clamp)
    {
        _motionBlur.active = true;
        _motionBlur.intensity.value = intensity;
        _motionBlur.clamp.value = clamp;
    }
    public void DisableMotionBlur()
    {
        _motionBlur.active = false;
    }

    #region Old Post-Processing
    //// Old Blur for Post-Processing.
    //public void StartRadialBlur(Ease ease, params RadialBlurSetting[] settings)
    //{
    //    Sequence effectSequence = DOTween.Sequence();

    //    foreach (var setting in settings)
    //    {
    //        effectSequence
    //            .Append(DOTween.To(() => _radialBlur.m_Strength.value, x => _radialBlur.m_Strength.value = x, setting.strength, setting.duration).SetEase(ease))
    //            .Insert(0f, DOTween.To(() => _radialBlur.m_Dist.value, x => _radialBlur.m_Dist.value = x, setting.dist, setting.duration).SetEase(ease));
    //    }

    //    effectSequence.Play();
    //}
    #endregion
}

[Serializable]
public struct RadialBlurSetting
{
    public float strength;
    public float dist;
    public float duration;
}

[Serializable]
public struct VignetteSetting
{
    public Color color;
    public float intensity;
    public float smoothness;
    public float duration;
}

[Serializable]
public struct MotionBlurSetting
{
    public float intensity;
    public float clamp;
}
