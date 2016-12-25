using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class LifeStageAge
	{
		public LifeStageDef def;

		public float minAge;

		public SoundDef soundCall;

		public SoundDef soundAngry;

		public SoundDef soundWounded;

		public SoundDef soundDeath;

		private static readonly Texture2D VeryYoungIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/VeryYoung", true);

		private static readonly Texture2D YoungIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Young", true);

		private static readonly Texture2D AdultIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Adult", true);

		public Texture2D GetIcon(Pawn forPawn)
		{
			if (this.def.iconTex != null)
			{
				return this.def.iconTex;
			}
			int count = forPawn.RaceProps.lifeStageAges.Count;
			int num = forPawn.RaceProps.lifeStageAges.IndexOf(this);
			if (num == count - 1)
			{
				return LifeStageAge.AdultIcon;
			}
			if (num == 0)
			{
				return LifeStageAge.VeryYoungIcon;
			}
			return LifeStageAge.YoungIcon;
		}
	}
}
