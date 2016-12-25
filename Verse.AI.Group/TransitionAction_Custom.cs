using System;

namespace Verse.AI.Group
{
	public class TransitionAction_Custom : TransitionAction
	{
		public Action action;

		public Action<Transition> actionWithArg;

		public TransitionAction_Custom(Action action)
		{
			this.action = action;
		}

		public TransitionAction_Custom(Action<Transition> actionWithArg)
		{
			this.actionWithArg = actionWithArg;
		}

		public override void DoAction(Transition trans)
		{
			if (this.actionWithArg != null)
			{
				this.actionWithArg(trans);
			}
			if (this.action != null)
			{
				this.action();
			}
		}
	}
}
