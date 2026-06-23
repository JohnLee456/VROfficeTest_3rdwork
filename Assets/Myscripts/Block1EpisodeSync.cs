using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public static class Block1EpisodeSync
{
    public const byte EpisodeStartedEventCode = 71;
    public const string BlockKey = "blockNumber";
    public const string TrialKey = "block1Trial";
    public const string EpisodeKey = "block1Episode";
    public const string EpisodeStartTimeKey = "block1EpisodeStartTime";

    public static void BroadcastEpisodeStarted(int trialNumber, int episodeNumber)
    {
        BroadcastEpisodeStarted(1, trialNumber, episodeNumber);
    }

    public static void BroadcastEpisodeStarted(int blockNumber, int trialNumber, int episodeNumber)
    {
        double startTime = PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.time;
        object[] payload = { blockNumber, trialNumber, episodeNumber, startTime };

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.RaiseEvent(
                EpisodeStartedEventCode,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                SendOptions.SendReliable);

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
            {
                { BlockKey, blockNumber },
                { TrialKey, trialNumber },
                { EpisodeKey, episodeNumber },
                { EpisodeStartTimeKey, startTime }
            });
        }
    }

    public static bool TryParsePayload(object eventContent, out int blockNumber, out int trialNumber, out int episodeNumber, out double startTime)
    {
        blockNumber = 1;
        trialNumber = 0;
        episodeNumber = 0;
        startTime = 0d;

        object[] payload = eventContent as object[];
        if (payload == null || payload.Length < 3)
        {
            return false;
        }

        if (payload.Length >= 4)
        {
            blockNumber = System.Convert.ToInt32(payload[0]);
            trialNumber = System.Convert.ToInt32(payload[1]);
            episodeNumber = System.Convert.ToInt32(payload[2]);
            startTime = System.Convert.ToDouble(payload[3]);
        }
        else
        {
            trialNumber = System.Convert.ToInt32(payload[0]);
            episodeNumber = System.Convert.ToInt32(payload[1]);
            startTime = System.Convert.ToDouble(payload[2]);
        }

        return true;
    }

    public static bool TryReadRoomState(out int blockNumber, out int trialNumber, out int episodeNumber, out double startTime)
    {
        blockNumber = 1;
        trialNumber = 0;
        episodeNumber = 0;
        startTime = 0d;

        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            return false;
        }

        Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!properties.ContainsKey(TrialKey) || !properties.ContainsKey(EpisodeKey) || !properties.ContainsKey(EpisodeStartTimeKey))
        {
            return false;
        }

        if (properties.ContainsKey(BlockKey))
        {
            blockNumber = System.Convert.ToInt32(properties[BlockKey]);
        }

        trialNumber = System.Convert.ToInt32(properties[TrialKey]);
        episodeNumber = System.Convert.ToInt32(properties[EpisodeKey]);
        startTime = System.Convert.ToDouble(properties[EpisodeStartTimeKey]);
        return true;
    }
}
