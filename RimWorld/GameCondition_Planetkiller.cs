using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class GameCondition_Planetkiller : GameCondition
	{
		private const int SoundDuration = 179;

		private const int FadeDuration = 90;

		private static readonly Color FadeColor = Color.white;

		public override string TooltipString
		{
			get
			{
				string text = this.def.LabelCap;
				text += "\n";
				text = text + "\n" + this.def.description;
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					"\n",
					"ImpactDate".Translate().CapitalizeFirst(),
					": ",
					GenDate.DateFullStringAt((long)GenDate.TickGameToAbs(this.startTick + base.Duration), Find.WorldGrid.LongLatOf(base.Map.Tile))
				});
				text2 = text;
				return string.Concat(new string[]
				{
					text2,
					"\n",
					"TimeLeft".Translate().CapitalizeFirst(),
					": ",
					base.TicksLeft.ToStringTicksToPeriod(true, false, true)
				});
			}
		}

		public override void GameConditionTick()
		{
			base.GameConditionTick();
			if (base.TicksLeft <= 179)
			{
				Find.ActiveLesson.Deactivate();
				if (base.TicksLeft == 179)
				{
					SoundDefOf.PlanetkillerImpact.PlayOneShotOnCamera(null);
				}
				if (base.TicksLeft == 90)
				{
					ScreenFader.StartFade(GameCondition_Planetkiller.FadeColor, 1f);
				}
			}
		}

		public override void End()
		{
			base.End();
			this.Impact();
		}

		private void Impact()
		{
			ScreenFader.SetColor(Color.clear);
			GenGameEnd.EndGameDialogMessage("GameOverPlanetkillerImpact".Translate(new object[]
			{
				Find.World.info.name
			}), false, GameCondition_Planetkiller.FadeColor);
		}
	}
}
