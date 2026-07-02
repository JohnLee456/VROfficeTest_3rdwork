using System;
using UnityEngine;

public class RoleAvatarIdentity : MonoBehaviour
{
    public static readonly string[] KnownAvatarIds = { "GCHbot", "ZJR", "ZHZ", "DCY" };

    [SerializeField] private string avatarId;

    public string AvatarId
    {
        get
        {
            return string.IsNullOrWhiteSpace(avatarId) ? NormalizeAvatarId(gameObject.name) : avatarId.Trim();
        }
    }

    public void InitializeIfEmpty(string id)
    {
        if (!string.IsNullOrWhiteSpace(avatarId))
        {
            return;
        }

        avatarId = NormalizeAvatarId(id);
    }

    public bool Matches(string id)
    {
        return MatchesAvatarId(AvatarId, id);
    }

    public static bool MatchesAvatarId(string left, string right)
    {
        return string.Equals(NormalizeAvatarId(left), NormalizeAvatarId(right), StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeAvatarId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        return id.Replace("(Clone)", string.Empty).Trim();
    }
}
