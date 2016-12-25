using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_BedroomJealous : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.IsColonist)
			{
				return false;
			}
			float num = 0f;
			Room ownedRoom = p.ownership.OwnedRoom;
			if (ownedRoom != null)
			{
				num = ownedRoom.GetStat(RoomStatDefOf.Impressiveness);
			}
			List<Pawn> list = p.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].HostFaction == null && p.RaceProps.Humanlike && list[i].ownership != null)
				{
					Room ownedRoom2 = list[i].ownership.OwnedRoom;
					if (ownedRoom2 != null)
					{
						float stat = ownedRoom2.GetStat(RoomStatDefOf.Impressiveness);
						if (stat - num >= Mathf.Abs(num * 0.1f))
						{
							return ThoughtState.ActiveWithReason(list[i].LabelShort);
						}
					}
				}
			}
			return false;
		}
	}
}
