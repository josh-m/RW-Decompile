using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class FactionBaseAbandonUtility
	{
		private static readonly Texture2D AbandonCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/AbandonHome", true);

		public static Command AbandonCommand(FactionBase factionBase)
		{
			Map map = factionBase.Map;
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandAbandonHome".Translate();
			command_Action.defaultDesc = "CommandAbandonHomeDesc".Translate();
			command_Action.icon = FactionBaseAbandonUtility.AbandonCommandTex;
			command_Action.action = delegate
			{
				if (map == null)
				{
					FactionBaseAbandonUtility.Abandon(factionBase);
					SoundDefOf.TickHigh.PlayOneShotOnCamera();
				}
				else
				{
					IEnumerable<Pawn> source = map.mapPawns.PawnsInFaction(Faction.OfPlayer);
					if (source.Count<Pawn>() == 0)
					{
						FactionBaseAbandonUtility.Abandon(factionBase);
						SoundDefOf.TickHigh.PlayOneShotOnCamera();
					}
					else
					{
						StringBuilder stringBuilder = new StringBuilder();
						foreach (Pawn current in from x in source
						orderby x.IsColonist descending
						select x)
						{
							stringBuilder.AppendLine("    " + current.LabelCap);
						}
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmAbandonHomeWithColonyPawns".Translate(new object[]
						{
							stringBuilder
						}), delegate
						{
							FactionBaseAbandonUtility.Abandon(factionBase);
						}, false, null));
					}
				}
			};
			if (!CaravanUtility.PlayerHasAnyCaravan() && !Find.Maps.Any((Map x) => x.info.parent != factionBase && x.mapPawns.FreeColonistsSpawned.Any<Pawn>()))
			{
				command_Action.Disable("CommandAbandonHomeFailAllColonistsThere".Translate());
			}
			return command_Action;
		}

		private static void Abandon(FactionBase factionBase)
		{
			Find.WorldObjects.Remove(factionBase);
			FactionBaseAbandonUtility.AddAbandonedBase(factionBase);
			Find.GameEnder.CheckGameOver();
		}

		private static void AddAbandonedBase(FactionBase factionBase)
		{
			WorldObject worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedFactionBase);
			worldObject.Tile = factionBase.Tile;
			worldObject.SetFaction(factionBase.Faction);
			Find.WorldObjects.Add(worldObject);
		}
	}
}
