﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Common;
using Darkages.Network.Object;

#endregion

namespace Darkages.Types
{
    public class Party : ObjectManager
    {
        public int Id { get; set; }
        public string LeaderName { get; set; }

        public List<Aisling> PartyMembers => GetObjects<Aisling>(null, sprite => sprite.GroupId == Id)
            .DistinctBy(i => i.Username.ToLower()).ToList();

        public static bool AddPartyMember(Aisling partyLeader, Aisling playerToAdd)
        {
            if (playerToAdd == null)
                return false;

            if (partyLeader.GroupId != 0)
            {
                if (playerToAdd.GroupId != 0 && playerToAdd.GroupId != partyLeader.GroupId)
                {
                    partyLeader.Client.SystemMessage(
                        $"{playerToAdd.Username} belongs to another party, and was not able to join your party.");
                    playerToAdd.Client.SystemMessage(
                        $"{partyLeader.Username}'s requested you to join his party. However you belong to another party.");

                    return false;
                }

                if (playerToAdd.GroupId != 0 || partyLeader.GroupId == 0)
                    return false;

                playerToAdd.GroupId = partyLeader.GroupId;

                partyLeader.Client.SystemMessage($"{playerToAdd.Username} has joined your party.");
                playerToAdd.Client.SystemMessage($"You have joined {partyLeader.Username}'s party.");

                return true;
            }

            if (playerToAdd.GroupId != 0 && partyLeader.GroupId == 0)
            {
                playerToAdd.Client.SystemMessage(
                    $"{partyLeader.Username} belongs to another party, and was not able to join your party.");

                partyLeader.Client.SystemMessage(
                    $"{playerToAdd}'s requested you to join his party. However you belong to another party.");

                return false;
            }

            if (playerToAdd.GroupId != 0 || partyLeader.GroupId != 0)
                return false;

            var party = CreateParty(partyLeader);
            playerToAdd.GroupId = party.Id;

            foreach (var player in party.PartyMembers)
                player.Client.SystemMessage($"{playerToAdd.Username} has joined the party.");

            playerToAdd.Client.SystemMessage($"You have joined {partyLeader.Username}'s party.");
            playerToAdd.GroupId = party.Id;

            return true;
        }

        public static Party CreateParty(Aisling partyLeader)
        {
            if (partyLeader == null) throw new ArgumentNullException(nameof(partyLeader));

            if (partyLeader.GroupId != 0)
                return null;

            var party = new Party {LeaderName = partyLeader.Username};

            lock (Generator.Random)
            {
                var pendingId = Generator.GenerateNumber();

                party.Id = pendingId;
                party.LeaderName = partyLeader.Username;
                partyLeader.GroupId = party.Id;

                if (!ServerContext.GlobalGroupCache.ContainsKey(party.Id))
                    ServerContext.GlobalGroupCache.Add(party.Id, party);
            }

            return party;
        }

        public static void DisbandParty(Party group)
        {
            if (!ServerContext.GlobalGroupCache.ContainsKey(group.Id)) return;
            if (!ServerContext.GlobalGroupCache.Remove(group.Id)) return;
            foreach (var player in group.PartyMembers)
            {
                player.GroupId = 0;
                player.Client.SendMessage("The party has now been disbanded.");
            }
        }

        public static void RemovePartyMember(Aisling playerToRemove)
        {
            if (ServerContext.GlobalGroupCache.ContainsKey(playerToRemove.GroupId))
            {
                var group = ServerContext.GlobalGroupCache[playerToRemove.GroupId];

                if (group != null)
                {
                    foreach (var player in group.PartyMembers)
                        player.Client.SendMessage($"{playerToRemove.Username} has left the party.");

                    playerToRemove.GroupId = 0;

                    if (group.PartyMembers.Count <= 1)
                    {
                        DisbandParty(group);
                    }
                    else
                    {
                        var nextPlayer = group.PartyMembers.FirstOrDefault();

                        if (nextPlayer == null)
                            return;

                        group.LeaderName = nextPlayer.Username;

                        foreach (var player in group.PartyMembers)
                            player.Client.SendMessage($"{nextPlayer.Username} is now the party leader.");
                    }
                }
            }
        }

        public bool Has(Aisling that)
        {
            return Id == that.GroupId;
        }
    }
}