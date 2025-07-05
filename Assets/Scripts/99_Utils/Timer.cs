using UnityEngine;

public class Timer
{
    const float NoTimeLimit = 0;
    public float time { get; private set; }
    private float timeUp = NoTimeLimit;
    public float remainTime => timeUp - time;
    public float processRatio => time / timeUp;

    public Timer(float timeUp = NoTimeLimit)
    {
        Initialize(timeUp);
    }

    public void Initialize(float timeUp = NoTimeLimit)
    {
        if (timeUp > 0)
        {
            this.timeUp = timeUp;
        }
        time = 0;
    }

    public void SetRemainTimeZero() { time = timeUp; }

    public bool Tick()
    {
        time += Time.deltaTime;
        return remainTime > 0;
    }

    public bool UnScaledTick()
    {
        time += Time.unscaledDeltaTime;
        return remainTime > 0;
    }
}