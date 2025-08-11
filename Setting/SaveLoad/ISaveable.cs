// ISaveable.cs
public interface ISaveable
{
    string UniqueID { get; }
    object CaptureState();
    void RestoreState(object state);
}

/// <summary>
/// 모든 RestoreState가 끝나고 '다음 프레임'에 한 번 호출(겹침 정리 등 후처리용)
/// </summary>
public interface IPostLoad
{
    void OnPostLoad();
}