using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
				List<Verb> verbs = this.AllVerbs;
				for (int i = 0; i < verbs.Count; i++)
				{
					Verb verb = verbs[i];
					if (verb.verbProps.hasStandardCommand)
					{
						Command_VerbTarget newOpt = new Command_VerbTarget();
						newOpt.defaultDesc = ownerThing.LabelCap + ": " + ownerThing.def.description;
						newOpt.icon = ownerThing.def.uiIcon;
						newOpt.tutorTag = "VerbTarget";
						newOpt.verb = verb;
						if (verb.caster.Faction != Faction.OfPlayer)
						{
							newOpt.Disable("CannotOrderNonControlled".Translate());
						}
						if (verb.CasterIsPawn)
						{
							if (verb.CasterPawn.story.WorkTagIsDisabled(WorkTags.Violent))
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
			Scribe_Collections.Look<Verb>(ref this.verbs, "verbs", LookMode.Deep, new object[0]);
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
				List<VerbProperties> verbProperties = this.directOwner.VerbProperties;
				for (int i = 0; i < verbProperties.Count; i++)
				{
					try
					{
						VerbProperties verbProperties2 = verbProperties[i];
						Verb verb = (Verb)Activator.CreateInstance(verbProperties2.verbClass);
						verb.loadID = Find.World.uniqueIDsManager.GetNextVerbID();
						this.verbs.Add(verb);
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new object[]
						{
							"Could not instantiate Verb (directOwner=",
							this.directOwner.ToStringSafe<IVerbOwner>(),
							"): ",
							ex
						}));
					}
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
			if (this.verbs.Count != verbProperties.Count)
			{
				Log.Error("Verbs count is not equal to verb props count.");
				while (this.verbs.Count > verbProperties.Count)
				{
					this.verbs.RemoveLast<Verb>();
				}
			}
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
