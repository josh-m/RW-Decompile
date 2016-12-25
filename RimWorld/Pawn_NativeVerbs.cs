using System;
using Verse;

namespace RimWorld
{
	public class Pawn_NativeVerbs : IExposable
	{
		private Pawn pawn;

		private Verb_BeatFire beatFireVerb;

		private Verb_Ignite igniteVerb;

		public Verb_BeatFire BeatFireVerb
		{
			get
			{
				if (this.beatFireVerb == null)
				{
					this.CreateVerbs();
				}
				return this.beatFireVerb;
			}
		}

		public Verb_Ignite IgniteVerb
		{
			get
			{
				if (this.igniteVerb == null)
				{
					this.CreateVerbs();
				}
				return this.igniteVerb;
			}
		}

		public Pawn_NativeVerbs(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void NativeVerbsTick()
		{
			if (this.BeatFireVerb != null)
			{
				this.BeatFireVerb.VerbTick();
			}
			if (this.IgniteVerb != null)
			{
				this.IgniteVerb.VerbTick();
			}
		}

		public bool TryStartIgnite(Thing target)
		{
			if (this.IgniteVerb == null)
			{
				Log.ErrorOnce(string.Concat(new object[]
				{
					this.pawn,
					" tried to ignite ",
					target,
					" but has no ignite verb."
				}), 76453432);
				return false;
			}
			return !this.pawn.stances.FullBodyBusy && this.IgniteVerb.TryStartCastOn(target, false, true);
		}

		public bool TryBeatFire(Fire targetFire)
		{
			if (this.BeatFireVerb == null)
			{
				Log.ErrorOnce(string.Concat(new object[]
				{
					this.pawn,
					" tried to beat fire ",
					targetFire,
					" but has no beat fire verb."
				}), 935137531);
				return false;
			}
			return !this.pawn.stances.FullBodyBusy && this.BeatFireVerb.TryStartCastOn(targetFire, false, true);
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<Verb_BeatFire>(ref this.beatFireVerb, "beatFireVerb", new object[0]);
			Scribe_Deep.LookDeep<Verb_Ignite>(ref this.igniteVerb, "igniteVerb", new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.UpdateVerbsLinksAndProps();
			}
		}

		private void CreateVerbs()
		{
			if (this.pawn.RaceProps.intelligence >= Intelligence.ToolUser)
			{
				UniqueIDsManager uniqueIDsManager = Find.World.uniqueIDsManager;
				this.beatFireVerb = new Verb_BeatFire();
				this.beatFireVerb.loadID = uniqueIDsManager.GetNextVerbID();
				if (!this.pawn.RaceProps.IsMechanoid)
				{
					this.igniteVerb = new Verb_Ignite();
					this.igniteVerb.loadID = uniqueIDsManager.GetNextVerbID();
				}
				this.UpdateVerbsLinksAndProps();
			}
		}

		private void UpdateVerbsLinksAndProps()
		{
			if (this.beatFireVerb != null)
			{
				this.beatFireVerb.caster = this.pawn;
				this.beatFireVerb.verbProps = NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.BeatFire);
			}
			if (this.igniteVerb != null)
			{
				this.igniteVerb.caster = this.pawn;
				this.igniteVerb.verbProps = NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.Ignite);
			}
		}
	}
}
