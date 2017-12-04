using System;
using System.Collections.Generic;

namespace Verse
{
	public abstract class ChoiceLetter : LetterWithTimeout
	{
		public string title;

		public string text;

		public bool radioMode;

		protected abstract IEnumerable<DiaOption> Choices
		{
			get;
		}

		protected DiaOption Reject
		{
			get
			{
				return new DiaOption("RejectLetter".Translate())
				{
					action = delegate
					{
						Find.LetterStack.RemoveLetter(this);
					},
					resolveTree = true
				};
			}
		}

		protected DiaOption Postpone
		{
			get
			{
				DiaOption diaOption = new DiaOption("PostponeLetter".Translate());
				diaOption.resolveTree = true;
				if (base.TimeoutActive && this.disappearAtTick <= Find.TickManager.TicksGame + 1)
				{
					diaOption.Disable(null);
				}
				return diaOption;
			}
		}

		protected DiaOption OK
		{
			get
			{
				return new DiaOption("OK".Translate())
				{
					action = delegate
					{
						Find.LetterStack.RemoveLetter(this);
					},
					resolveTree = true
				};
			}
		}

		protected DiaOption JumpToLocation
		{
			get
			{
				DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
				diaOption.action = delegate
				{
					CameraJumper.TryJumpAndSelect(this.lookTarget);
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				if (!this.lookTarget.IsValid)
				{
					diaOption.Disable(null);
				}
				return diaOption;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.title, "title", null, false);
			Scribe_Values.Look<string>(ref this.text, "text", null, false);
			Scribe_Values.Look<bool>(ref this.radioMode, "radioMode", false, false);
		}

		protected override string GetMouseoverText()
		{
			return this.text;
		}

		public override void OpenLetter()
		{
			DiaNode diaNode = new DiaNode(this.text);
			diaNode.options.AddRange(this.Choices);
			WindowStack arg_39_0 = Find.WindowStack;
			DiaNode nodeRoot = diaNode;
			bool flag = this.radioMode;
			arg_39_0.Add(new Dialog_NodeTree(nodeRoot, false, flag, this.title));
		}
	}
}
