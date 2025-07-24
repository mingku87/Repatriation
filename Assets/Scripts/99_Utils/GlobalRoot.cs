public class GlobalRoot : SingletonObject<GlobalRoot>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}