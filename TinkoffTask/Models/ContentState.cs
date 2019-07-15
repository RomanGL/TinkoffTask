namespace TinkoffTask
{
    /// <summary>
    /// Перечисление состояний контента.
    /// </summary>
    public enum ContentState : byte
    {
        None = 0,
        Normal,
        Loading,
        Error,
        NoData
    }
}
