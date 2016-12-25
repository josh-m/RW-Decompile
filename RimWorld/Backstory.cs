using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Backstory
	{
		public string identifier;

		public BackstorySlot slot;

		private string title;

		private string titleShort;

		public string baseDesc;

		public Dictionary<string, int> skillGains = new Dictionary<string, int>();

		public Dictionary<SkillDef, int> skillGainsResolved = new Dictionary<SkillDef, int>();

		public WorkTags workDisables;

		public WorkTags requiredWorkTags;

		public List<string> spawnCategories = new List<string>();

		[LoadAlias("bodyNameGlobal")]
		public BodyType bodyTypeGlobal;

		[LoadAlias("bodyNameFemale")]
		public BodyType bodyTypeFemale;

		[LoadAlias("bodyNameMale")]
		public BodyType bodyTypeMale;

		public List<TraitEntry> forcedTraits;

		public List<TraitEntry> disallowedTraits;

		public bool shuffleable = true;

		public IEnumerable<WorkTypeDef> DisabledWorkTypes
		{
			get
			{
				List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
				for (int i = 0; i < list.Count; i++)
				{
					if (!this.AllowsWorkType(list[i]))
					{
						yield return list[i];
					}
				}
			}
		}

		public string Title
		{
			get
			{
				return this.title;
			}
		}

		public string TitleShort
		{
			get
			{
				if (!this.titleShort.NullOrEmpty())
				{
					return this.titleShort;
				}
				return this.title;
			}
		}

		public bool DisallowsTrait(TraitDef def, int degree)
		{
			if (this.disallowedTraits == null)
			{
				return false;
			}
			for (int i = 0; i < this.disallowedTraits.Count; i++)
			{
				if (this.disallowedTraits[i].def == def && this.disallowedTraits[i].degree == degree)
				{
					return true;
				}
			}
			return false;
		}

		public BodyType BodyTypeFor(Gender g)
		{
			if (this.bodyTypeGlobal != BodyType.Undefined || g == Gender.None)
			{
				return this.bodyTypeGlobal;
			}
			if (g == Gender.Female)
			{
				return this.bodyTypeFemale;
			}
			return this.bodyTypeMale;
		}

		public string FullDescriptionFor(Pawn p)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.baseDesc.AdjustedFor(p));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				if (this.skillGainsResolved.ContainsKey(skillDef))
				{
					stringBuilder.AppendLine(skillDef.skillLabel + ":   " + this.skillGainsResolved[skillDef].ToString("+##;-##"));
				}
			}
			foreach (WorkTypeDef current in this.DisabledWorkTypes)
			{
				stringBuilder.AppendLine(current.gerundLabel + " " + "DisabledLower".Translate());
			}
			return stringBuilder.ToString();
		}

		private bool AllowsWorkType(WorkTypeDef workDef)
		{
			return (this.workDisables & workDef.workTags) == WorkTags.None;
		}

		internal void AddForcedTrait(TraitDef traitDef, int degree = 0)
		{
			if (this.forcedTraits == null)
			{
				this.forcedTraits = new List<TraitEntry>();
			}
			this.forcedTraits.Add(new TraitEntry(traitDef, degree));
		}

		internal void AddDisallowedTrait(TraitDef traitDef, int degree = 0)
		{
			if (this.disallowedTraits == null)
			{
				this.disallowedTraits = new List<TraitEntry>();
			}
			this.disallowedTraits.Add(new TraitEntry(traitDef, degree));
		}

		public void PostLoad()
		{
			if (!this.title.Equals(GenText.ToNewsCase(this.title)))
			{
				Log.Warning("Bad capitalization on backstory title: " + this.title);
				this.title = GenText.ToNewsCase(this.title);
			}
			if (this.slot == BackstorySlot.Adulthood && this.bodyTypeGlobal == BodyType.Undefined)
			{
				if (this.bodyTypeMale == BodyType.Undefined)
				{
					Log.Error("Adulthood backstory " + this.title + " is missing male body type. Defaulting...");
					this.bodyTypeMale = BodyType.Male;
				}
				if (this.bodyTypeFemale == BodyType.Undefined)
				{
					Log.Error("Adulthood backstory " + this.title + " is missing female body type. Defaulting...");
					this.bodyTypeFemale = BodyType.Female;
				}
			}
			this.baseDesc = this.baseDesc.TrimEnd(new char[0]);
		}

		public void ResolveReferences()
		{
			int num = Mathf.Abs(GenText.StableStringHash(this.baseDesc) % 100);
			string s = this.title.Replace('-', ' ');
			s = GenText.CapitalizedNoSpaces(s);
			this.identifier = GenText.RemoveNonAlphanumeric(s) + num.ToString();
			foreach (KeyValuePair<string, int> current in this.skillGains)
			{
				this.skillGainsResolved.Add(DefDatabase<SkillDef>.GetNamed(current.Key, true), current.Value);
			}
			this.skillGains = null;
		}

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors(bool ignoreNoSpawnCategories)
		{
			if (this.title.NullOrEmpty())
			{
				yield return "null title, baseDesc is " + this.baseDesc;
			}
			if (this.titleShort.NullOrEmpty())
			{
				yield return "null titleShort, baseDesc is " + this.baseDesc;
			}
			if ((this.workDisables & WorkTags.Violent) != WorkTags.None && this.spawnCategories.Contains("Raider"))
			{
				yield return "cannot do Violent work but can spawn as a raider";
			}
			if (this.spawnCategories.Count == 0 && !ignoreNoSpawnCategories)
			{
				yield return "no spawn categories";
			}
			if (this.spawnCategories.Count == 1 && this.spawnCategories[0] == "Trader")
			{
				yield return "only Trader spawn category";
			}
			if (!this.baseDesc.NullOrEmpty())
			{
				if (char.IsWhiteSpace(this.baseDesc[0]))
				{
					yield return "baseDesc starts with whitepspace";
				}
				if (char.IsWhiteSpace(this.baseDesc[this.baseDesc.Length - 1]))
				{
					yield return "baseDesc ends with whitespace";
				}
			}
			if (Prefs.DevMode)
			{
				foreach (KeyValuePair<SkillDef, int> kvp in this.skillGainsResolved)
				{
					if ((kvp.Key.disablingWorkTags & this.workDisables) != WorkTags.None)
					{
						yield return "modifies skill " + kvp.Key + " but also disables this skill";
					}
				}
				foreach (KeyValuePair<string, Backstory> kvp2 in BackstoryDatabase.allBackstories)
				{
					if (kvp2.Value != this && kvp2.Value.identifier == this.identifier)
					{
						yield return "backstory identifier used more than once: " + this.identifier;
					}
				}
			}
		}

		public void SetTitle(string newTitle)
		{
			this.title = newTitle;
		}

		public void SetTitleShort(string newTitleShort)
		{
			this.titleShort = newTitleShort;
		}

		public override string ToString()
		{
			if (this.title.NullOrEmpty())
			{
				return "(NullTitleBackstory)";
			}
			return "(" + this.title + ")";
		}

		public override int GetHashCode()
		{
			return this.identifier.GetHashCode();
		}
	}
}
