namespace mvc.Services;

/// <summary>
/// Dienst zur Ermittlung des aktuellen Mandanten.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Liefert die ID des aktuellen Mandanten zurück (z. B. "main" oder "doc").
    /// </summary>
    string GetCurrentTenantId();
}
