using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class PawnBio
	{
		public GenderPossibility gender;

		public NameTriple name;

		public Backstory childhood;

		public Backstory adulthood;

		public bool pirateKing;

		public PawnBioType BioType
		{
			get
			{
				if (this.pirateKing)
				{
					return PawnBioType.PirateKing;
				}
				if (this.adulthood != null)
				{
					return PawnBioType.BackstoryInGame;
				}
				return PawnBioType.Undefined;
			}
		}

		public void PostLoad()
		{
			if (this.childhood != null)
			{
				this.childhood.PostLoad();
			}
			if (this.adulthood != null)
			{
				this.adulthood.PostLoad();
			}
		}

		public void ResolveReferences()
		{
			if (this.adulthood.spawnCategories.Count == 1 && this.adulthood.spawnCategories[0] == "Trader")
			{
				this.adulthood.spawnCategories.Add("Civil");
			}
			if (this.childhood != null)
			{
				this.childhood.ResolveReferences();
			}
			if (this.adulthood != null)
			{
				this.adulthood.ResolveReferences();
			}
		}

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors()
		{
			if (this.childhood != null)
			{
				foreach (string error in this.childhood.ConfigErrors(true))
				{
					yield return string.Concat(new object[]
					{
						this.name,
						", ",
						this.childhood.Title,
						": ",
						error
					});
				}
			}
			if (this.adulthood != null)
			{
				foreach (string error2 in this.adulthood.ConfigErrors(false))
				{
					yield return string.Concat(new object[]
					{
						this.name,
						", ",
						this.adulthood.Title,
						": ",
						error2
					});
				}
			}
		}

		public override string ToString()
		{
			return "PawnBio(" + this.name + ")";
		}
	}
}
