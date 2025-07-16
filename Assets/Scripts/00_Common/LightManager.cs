using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : SingletonObject<LightManager>
{
    private static Light2D globalLight;
    protected override void Awake()
    {
        base.Awake();
        if (globalLight == null) globalLight = GetComponent<Light2D>();
    }

    public static void ChangeToDayOrNight(TimeState timeState)
    {
        var targetIntensity = timeState == TimeState.Day ? InGameConstant.dayLightIntensity : InGameConstant.nightLightIntensity;
        Instance.StartCoroutine(ChangeLightIntensity(targetIntensity, InGameConstant.lightTransitionDuration));
    }

    private static IEnumerator ChangeLightIntensity(float targetIntensity, float duration)
    {
        if (globalLight == null) yield break;

        float startIntensity = globalLight.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        globalLight.intensity = targetIntensity;
    }
}