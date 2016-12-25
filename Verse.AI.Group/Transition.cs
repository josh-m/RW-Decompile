using System;
using System.Collections.Generic;

namespace Verse.AI.Group
{
	public class Transition
	{
		public List<LordToil> sources;

		public LordToil target;

		public List<Trigger> triggers = new List<Trigger>();

		public List<TransitionAction> preActions = new List<TransitionAction>();

		public List<TransitionAction> postActions = new List<TransitionAction>();

		public bool canMoveToSameState;

		public Map Map
		{
			get
			{
				return this.target.Map;
			}
		}

		public Transition(LordToil firstSource, LordToil target)
		{
			this.sources = new List<LordToil>();
			this.AddSource(firstSource);
			this.target = target;
		}

		public void AddSource(LordToil source)
		{
			if (this.sources.Contains(source))
			{
				Log.Error("Double-added source to Transition: " + source);
				return;
			}
			if (!this.canMoveToSameState && this.target == source)
			{
				Log.Error("Transition canMoveToSameState and target is source: " + source);
			}
			this.sources.Add(source);
		}

		public void AddSources(IEnumerable<LordToil> sources)
		{
			foreach (LordToil current in sources)
			{
				this.AddSource(current);
			}
		}

		public void AddSources(params LordToil[] sources)
		{
			for (int i = 0; i < sources.Length; i++)
			{
				this.AddSource(sources[i]);
			}
		}

		public void AddTrigger(Trigger trigger)
		{
			this.triggers.Add(trigger);
		}

		public void AddPreAction(TransitionAction action)
		{
			this.preActions.Add(action);
		}

		public void AddPostAction(TransitionAction action)
		{
			this.postActions.Add(action);
		}

		public void SourceToilBecameActive(Transition transition, LordToil previousToil)
		{
			for (int i = 0; i < this.triggers.Count; i++)
			{
				this.triggers[i].SourceToilBecameActive(transition, previousToil);
			}
		}

		public bool CheckSignal(Lord lord, TriggerSignal signal)
		{
			for (int i = 0; i < this.triggers.Count; i++)
			{
				if (this.triggers[i].ActivateOn(lord, signal))
				{
					if (DebugViewSettings.logLordToilTransitions)
					{
						Log.Message(string.Concat(new object[]
						{
							"Transitioning ",
							this.sources,
							" to ",
							this.target,
							" by trigger ",
							this.triggers[i],
							" on signal ",
							signal
						}));
					}
					this.Execute(lord);
					return true;
				}
			}
			return false;
		}

		public void Execute(Lord lord)
		{
			if (!this.canMoveToSameState && this.target == lord.CurLordToil)
			{
				return;
			}
			for (int i = 0; i < this.preActions.Count; i++)
			{
				this.preActions[i].DoAction(this);
			}
			lord.GotoToil(this.target);
			for (int j = 0; j < this.postActions.Count; j++)
			{
				this.postActions[j].DoAction(this);
			}
		}

		public override string ToString()
		{
			string text = (!this.sources.NullOrEmpty<LordToil>()) ? this.sources[0].ToString() : "null";
			int num = (this.sources != null) ? this.sources.Count : 0;
			string text2 = (this.target != null) ? this.target.ToString() : "null";
			return string.Concat(new object[]
			{
				text,
				"(",
				num,
				")->",
				text2
			});
		}
	}
}
