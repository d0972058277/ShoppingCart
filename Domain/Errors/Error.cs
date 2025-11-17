namespace Domain.Errors;

/// <summary>
/// 代表領域錯誤的不可變物件。
/// </summary>
/// <param name="Code">錯誤代碼（例如 "User.NotFound"）。</param>
/// <param name="Message">錯誤訊息。</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// 代表無錯誤的靜態實例。
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}
