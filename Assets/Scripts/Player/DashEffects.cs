using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DashEffects : MonoBehaviour
{
    [Header("Vignette Settings")]
    public Volume postProcessVolume;
    public float vignetteIntensity = 0.5f;
    public float vignetteDuration = 2f;
    
    private Vignette _vignette;
    private float _defaultVignetteIntensity;
    private bool _isVignettePulsing = false;
    private float _vignetteTimer = 0f;
    
    void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out _vignette))
        {
            _defaultVignetteIntensity = _vignette.intensity.value;
        }
    }
    
    void Update()
    {
        if (_isVignettePulsing)
        {
            _vignetteTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_vignetteTimer / vignetteDuration);
                        
            // Ease out from intense to default
            float intensity = Mathf.Lerp(vignetteIntensity, _defaultVignetteIntensity, progress);
            
            if (_vignette != null)
            {
                _vignette.intensity.value = intensity;
            }
            
            if (_vignetteTimer >= vignetteDuration)
            {
                _isVignettePulsing = false;
                if (_vignette != null)
                {
                    _vignette.intensity.value = _defaultVignetteIntensity;
                }
            }
        }
    }
    
    public void TriggerVignettePulse()
    {
        _isVignettePulsing = true;
        _vignetteTimer = 0f;
        
        if (_vignette != null)
        {
            _vignette.intensity.value = vignetteIntensity;
        }
    }
    
    public void TriggerVignettePulse(float duration)
    {
        vignetteDuration = duration;
        TriggerVignettePulse();
    }
}
