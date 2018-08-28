using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ArchivedDialog : IArchivable, IExposable, ILoadReferenceable
	{
		public int ID;

		public string text;

		public string title;

		public Faction relatedFaction;

		public int createdTick;

		Texture IArchivable.ArchivedIcon
		{
			get
			{
				return null;
			}
		}

		Color IArchivable.ArchivedIconColor
		{
			get
			{
				return Color.white;
			}
		}

		string IArchivable.ArchivedLabel
		{
			get
			{
				return this.text.Flatten();
			}
		}

		string IArchivable.ArchivedTooltip
		{
			get
			{
				return this.text;
			}
		}

		int IArchivable.CreatedTicksGame
		{
			get
			{
				return this.createdTick;
			}
		}

		bool IArchivable.CanCullArchivedNow
		{
			get
			{
				return true;
			}
		}

		LookTargets IArchivable.LookTargets
		{
			get
			{
				return null;
			}
		}

		public ArchivedDialog()
		{
		}

		public ArchivedDialog(string text, string title = null, Faction relatedFaction = null)
		{
			this.text = text;
			this.title = title;
			this.relatedFaction = relatedFaction;
			this.createdTick = GenTicks.TicksGame;
			if (Find.UniqueIDsManager != null)
			{
				this.ID = Find.UniqueIDsManager.GetNextArchivedDialogID();
			}
			else
			{
				this.ID = Rand.Int;
			}
		}

		void IArchivable.OpenArchived()
		{
			DiaNode diaNode = new DiaNode(this.text);
			DiaOption diaOption = new DiaOption("OK".Translate());
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			WindowStack arg_50_0 = Find.WindowStack;
			DiaNode nodeRoot = diaNode;
			Faction faction = this.relatedFaction;
			string text = this.title;
			arg_50_0.Add(new Dialog_NodeTreeWithFactionInfo(nodeRoot, faction, false, false, text));
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.ID, "ID", 0, false);
			Scribe_Values.Look<string>(ref this.text, "text", null, false);
			Scribe_Values.Look<string>(ref this.title, "title", null, false);
			Scribe_References.Look<Faction>(ref this.relatedFaction, "relatedFaction", false);
			Scribe_Values.Look<int>(ref this.createdTick, "createdTick", 0, false);
		}

		public string GetUniqueLoadID()
		{
			return "ArchivedDialog_" + this.ID;
		}
	}
}
