using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class LightColor
{
    public TimeState timeState;
    public Color color;
    public float intensity;
}

public class LightManager : SingletonObject<LightManager>
{
    private static Light2D globalLight;
    [SerializeField] private LightColor[] lights;

    protected override void Awake()
    {
        base.Awake();
        if (globalLight == null) globalLight = GetComponent<Light2D>();
    }

    public static void ChangeTimeState(TimeState timeState)
    {
        var target = Instance.GetPreset(timeState);
        Instance.StopAllCoroutines();
        Instance.StartCoroutine(ChangeLight(target));
    }

    private LightColor GetPreset(TimeState state)
    {
        foreach (var light in lights) if (light.timeState == state) return light;
        return null;
    }

    private static IEnumerator ChangeLight(LightColor targetLightColor)
    {
        var startColor = globalLight.color;
        var startIntensity = globalLight.intensity;

        float timer = 0.0f;
        float duration = InGameConstant.lightTransitionDuration;

        while (timer < duration)
        {
            globalLight.color = Color.Lerp(startColor, targetLightColor.color, timer / duration);
            globalLight.intensity = Mathf.Lerp(startIntensity, targetLightColor.intensity, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        globalLight.color = targetLightColor.color;
        globalLight.intensity = targetLightColor.intensity;
    }
}