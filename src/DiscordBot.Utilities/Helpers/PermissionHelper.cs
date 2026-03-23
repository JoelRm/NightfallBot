namespace DiscordBot.Utilities.Helpers;

public static class PermissionHelper
{
    private static readonly ulong[] AdminRoleIds =
    {
        942603999285235712,
        1067904201818132530
    };

    public static bool EsAdmin(IEnumerable<ulong> rolesUsuario)
    {
        return rolesUsuario.Any(r => AdminRoleIds.Contains(r));
    }
}