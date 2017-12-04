using System;
using Verse;

namespace RimWorld
{
	public static class BuildDesignatorUtility
	{
		public static void TryDrawPowerGridAndAnticipatedConnection(BuildableDef def)
		{
			ThingDef thingDef = def as ThingDef;
			if (thingDef != null && (thingDef.EverTransmitsPower || thingDef.ConnectToPower))
			{
				OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
				if (thingDef.ConnectToPower)
				{
					IntVec3 intVec = UI.MouseCell();
					CompPower compPower = PowerConnectionMaker.BestTransmitterForConnector(intVec, Find.VisibleMap, null);
					if (compPower != null && !compPower.parent.Position.Fogged(compPower.parent.Map))
					{
						PowerNetGraphics.RenderAnticipatedWirePieceConnecting(intVec, compPower.parent);
					}
				}
			}
		}
	}
}
