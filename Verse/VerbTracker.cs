using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class VerbTracker : IExposable
	{
		public IVerbOwner directOwner;

		private List<Verb> verbs;

		public List<Verb> AllVerbs
		{
			get
			{
				if (this.verbs == null)
				{
					this.InitVerbs();
				}
				return this.verbs;
			}
		}

		public Verb PrimaryVerb
		{
			get
			{
				if (this.verbs == null)
				{
					this.InitVerbs();
				}
				for (int i = 0; i < this.verbs.Count; i++)
				{
					if (this.verbs[i].verbProps.isPrimary)
					{
						return this.verbs[i];
					}
				}
				return null;
			}
		}

		public VerbTracker(IVerbOwner directOwner)
		{
			this.directOwner = directOwner;
		}

		public void VerbsTick()
		{
			if (this.verbs == null)
			{
				return;
			}
			for (int i = 0; i < this.verbs.Count; i++)
			{
				this.verbs[i].VerbTick();
			}
		}

		[DebuggerHidden]
		public IEnumerable<Command> GetVerbsCommands(KeyCode hotKey = KeyCode.None)
		{
			CompEquippable ce = this.directOwner as CompEquippable;
			if (ce != null)
			{
				Thing ownerThing = ce.parent;
				for (int i = 0; i < this.AllVerbs.Count; i++)
				{
					Verb verb = this.AllVerbs[i];
					if (verb.verbProps.hasStandardCommand)
					{
						Command_VerbTarget newOpt = new Command_VerbTarget();
						newOpt.defaultDesc = ownerThing.LabelCap + ": " + ownerThing.def.description;
						newOpt.icon = ownerThing.def.uiIcon;
						newOpt.verb = verb;
						if (verb.caster.Faction != Faction.OfPlayer)
						{
							newOpt.Disable("CannotOrderNonControlled".Translate());
						}
						if (verb.CasterIsPawn)
						{
							if (verb.CasterPawn.story.DisabledWorkTags.Contains(WorkTags.Violent))
							{
								newOpt.Disable("IsIncapableOfViolence".Translate(new object[]
								{
									verb.CasterPawn.NameStringShort
								}));
							}
							else if (!verb.CasterPawn.drafter.Drafted)
							{
								newOpt.Disable("IsNotDrafted".Translate(new object[]
								{
									verb.CasterPawn.NameStringShort
								}));
							}
						}
						yield return newOpt;
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Verb>(ref this.verbs, "verbs", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.UpdateVerbsLinksAndProps();
			}
		}

		private void InitVerbs()
		{
			if (this.verbs == null)
			{
				this.verbs = new List<Verb>();
				for (int i = 0; i < this.directOwner.VerbProperties.Count; i++)
				{
					VerbProperties verbProperties = this.directOwner.VerbProperties[i];
					Verb verb = (Verb)Activator.CreateInstance(verbProperties.verbClass);
					verb.loadID = Find.World.uniqueIDsManager.GetNextVerbID();
					this.verbs.Add(verb);
				}
				this.UpdateVerbsLinksAndProps();
			}
		}

		private void UpdateVerbsLinksAndProps()
		{
			if (this.verbs == null)
			{
				return;
			}
			List<VerbProperties> verbProperties = this.directOwner.VerbProperties;
			for (int i = 0; i < this.verbs.Count; i++)
			{
				Verb verb = this.verbs[i];
				verb.verbProps = verbProperties[i];
				CompEquippable compEquippable = this.directOwner as CompEquippable;
				Pawn pawn = this.directOwner as Pawn;
				HediffComp_VerbGiver hediffComp_VerbGiver = this.directOwner as HediffComp_VerbGiver;
				if (compEquippable != null)
				{
					verb.ownerEquipment = compEquippable.parent;
				}
				else if (pawn != null)
				{
					verb.caster = pawn;
				}
				else if (hediffComp_VerbGiver != null)
				{
					verb.ownerHediffComp = hediffComp_VerbGiver;
					verb.caster = hediffComp_VerbGiver.Pawn;
				}
			}
		}
	}
}
