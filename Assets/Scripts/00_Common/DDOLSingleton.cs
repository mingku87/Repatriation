public class DDOLSingleton<T> : SingletonObject<T> where T : SingletonObject<T>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}