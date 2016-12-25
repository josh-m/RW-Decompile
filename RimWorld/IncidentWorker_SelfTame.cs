using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_SelfTame : IncidentWorker
	{
		private IEnumerable<Pawn> Candidates(Map map)
		{
			return from x in map.mapPawns.AllPawnsSpawned
			where x.RaceProps.Animal && x.Faction == null && !x.Position.Fogged(x.Map) && !x.InMentalState && !x.Downed && x.RaceProps.wildness > 0f
			select x;
		}

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return this.Candidates(map).Any<Pawn>();
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Pawn pawn = null;
			if (!this.Candidates(map).TryRandomElementByWeight((Pawn x) => x.RaceProps.wildness, out pawn))
			{
				return false;
			}
			if (pawn.guest != null)
			{
				pawn.guest.SetGuestStatus(null, false);
			}
			string text = pawn.LabelIndefinite();
			bool flag = pawn.Name != null;
			pawn.SetFaction(Faction.OfPlayer, null);
			string text2;
			if (!flag && pawn.Name != null)
			{
				if (pawn.Name.Numerical)
				{
					text2 = "LetterAnimalSelfTameAndNameNumerical".Translate(new object[]
					{
						text,
						pawn.Name.ToStringFull
					}).CapitalizeFirst();
				}
				else
				{
					text2 = "LetterAnimalSelfTameAndName".Translate(new object[]
					{
						text,
						pawn.Name.ToStringFull
					}).CapitalizeFirst();
				}
			}
			else
			{
				text2 = "LetterAnimalSelfTame".Translate(new object[]
				{
					pawn.LabelIndefinite()
				}).CapitalizeFirst();
			}
			Find.LetterStack.ReceiveLetter("LetterLabelAnimalSelfTame".Translate(new object[]
			{
				GenLabel.BestKindLabel(pawn, false, false, false)
			}).CapitalizeFirst(), text2, LetterType.Good, pawn, null);
			return true;
		}
	}
}
