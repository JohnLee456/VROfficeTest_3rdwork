using System;
using System.Collections.Generic;

public static class LoginAccountResolver
{
    private const string OfficeLoggedIn = "OfficeLoggedIn";
    private const string OfficeLoggedInPath = "Assets/VR Office/Scenes/OfficeLoggedIn.unity";
    private const string OfficeLoggedInNoBot = "OfficeLoggedInNoBot";
    private const string OfficeLoggedInNoBotPath = "Assets/VR Office/Scenes/OfficeLoggedInNoBot.unity";

    private static readonly Dictionary<string, LoginRoute> Routes =
        new Dictionary<string, LoginRoute>(StringComparer.OrdinalIgnoreCase)
        {
            { "leader", new LoginRoute("leader", LoginUserRole.Leader, "GCHbot", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },
            { "study", new LoginRoute("study", LoginUserRole.Study, "GCHbot", OfficeLoggedIn, OfficeLoggedInPath) },

            { "zjr", new LoginRoute("zjr", LoginUserRole.Member, "ZJR", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },
            { "zhz", new LoginRoute("zhz", LoginUserRole.Member, "ZHZ", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },
            { "dcy", new LoginRoute("dcy", LoginUserRole.Member, "DCY", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },

            { "member1", new LoginRoute("member1", LoginUserRole.Member, "ZJR", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },
            { "member2", new LoginRoute("member2", LoginUserRole.Member, "ZHZ", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },
            { "member3", new LoginRoute("member3", LoginUserRole.Member, "DCY", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },
            { "member_zjr", new LoginRoute("member_zjr", LoginUserRole.Member, "ZJR", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },
            { "member_zhz", new LoginRoute("member_zhz", LoginUserRole.Member, "ZHZ", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) },
            { "member_dcy", new LoginRoute("member_dcy", LoginUserRole.Member, "DCY", OfficeLoggedInNoBot, OfficeLoggedInNoBotPath) }
        };

    public static bool TryResolve(string accountId, out LoginRoute route)
    {
        route = null;

        if (string.IsNullOrWhiteSpace(accountId))
        {
            return false;
        }

        return Routes.TryGetValue(accountId.Trim(), out route);
    }
}
