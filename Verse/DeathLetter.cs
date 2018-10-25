using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class DeathLetter : ChoiceLetter
	{
		protected DiaOption ReadMore
		{
			get
			{
				GlobalTargetInfo target = this.lookTargets.TryGetPrimaryTarget();
				DiaOption diaOption = new DiaOption("ReadMore".Translate());
				diaOption.action = delegate
				{
					CameraJumper.TryJumpAndSelect(target);
					Find.LetterStack.RemoveLetter(this);
					InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Log));
				};
				diaOption.resolveTree = true;
				if (!target.IsValid)
				{
					diaOption.Disable(null);
				}
				return diaOption;
			}
		}

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return base.Option_Close;
				if (this.lookTargets.IsValid())
				{
					yield return this.ReadMore;
				}
			}
		}

		public override void OpenLetter()
		{
			Pawn targetPawn = this.lookTargets.TryGetPrimaryTarget().Thing as Pawn;
			string text = this.text;
			string text2 = (from entry in (from entry in (from battle in Find.BattleLog.Battles
			where battle.Concerns(targetPawn)
			select battle).SelectMany((Battle battle) => from entry in battle.Entries
			where entry.Concerns(targetPawn) && entry.ShowInCompactView()
			select entry)
			orderby entry.Age
			select entry).Take(5).Reverse<LogEntry>()
			select "  " + entry.ToGameStringFromPOV(null, false)).ToLineList(null);
			if (text2.Length > 0)
			{
				text = string.Format("{0}\n\n{1}\n{2}", text, "LastEventsInLife".Translate(targetPawn.LabelDefinite(), targetPawn.Named("PAWN")) + ":", text2);
			}
			DiaNode diaNode = new DiaNode(text);
			diaNode.options.AddRange(this.Choices);
			WindowStack arg_13F_0 = Find.WindowStack;
			DiaNode nodeRoot = diaNode;
			Faction relatedFaction = this.relatedFaction;
			bool radioMode = this.radioMode;
			arg_13F_0.Add(new Dialog_NodeTreeWithFactionInfo(nodeRoot, relatedFaction, false, radioMode, this.title));
		}
	}
}
