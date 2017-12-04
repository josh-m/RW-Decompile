using RimWorld;
using System;
using System.Linq;

namespace Verse.AI.Group
{
	public class TransitionAction_Message : TransitionAction
	{
		public string message;

		public MessageTypeDef type;

		public TargetInfo lookTarget;

		public TransitionAction_Message(string message) : this(message, MessageTypeDefOf.NeutralEvent)
		{
			this.message = message;
		}

		public TransitionAction_Message(string message, MessageTypeDef messageType)
		{
			this.lookTarget = TargetInfo.Invalid;
			base..ctor();
			this.message = message;
			this.type = messageType;
		}

		public TransitionAction_Message(string message, MessageTypeDef messageType, TargetInfo lookTarget)
		{
			this.lookTarget = TargetInfo.Invalid;
			base..ctor();
			this.message = message;
			this.type = messageType;
			this.lookTarget = lookTarget;
		}

		public override void DoAction(Transition trans)
		{
			TargetInfo target;
			if (this.lookTarget.IsValid)
			{
				target = this.lookTarget;
			}
			else
			{
				target = trans.target.lord.ownedPawns.FirstOrDefault<Pawn>();
			}
			Messages.Message(this.message, target, this.type);
		}
	}
}
