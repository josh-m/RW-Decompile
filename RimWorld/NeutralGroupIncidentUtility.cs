using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class NeutralGroupIncidentUtility
	{
		public static bool AnyBlockingHostileLord(Map map, Faction forFaction)
		{
			Faction faction = map.ParentFaction ?? Faction.OfPlayer;
			List<Lord> lords = map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				if (lords[i].faction != null && lords[i].faction != forFaction && lords[i].faction != faction)
				{
					if (lords[i].AnyActivePawn)
					{
						LordJob lordJob = lords[i].LordJob;
						if (lordJob == null || lordJob.CanBlockHostileVisitors)
						{
							if (!(lordJob is LordJob_VoluntarilyJoinable))
							{
								if (lords[i].faction.HostileTo(forFaction))
								{
									if (!lords[i].faction.HostileTo(faction))
									{
										return true;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}
	}
}
