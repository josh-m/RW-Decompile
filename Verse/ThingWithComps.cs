using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class ThingWithComps : Thing
	{
		private List<ThingComp> comps = new List<ThingComp>();

		public List<ThingComp> AllComps
		{
			get
			{
				return this.comps;
			}
		}

		public override Color DrawColor
		{
			get
			{
				CompColorable comp = this.GetComp<CompColorable>();
				if (comp != null && comp.Active)
				{
					return comp.Color;
				}
				return base.DrawColor;
			}
			set
			{
				this.SetColor(value, true);
			}
		}

		public override string LabelNoCount
		{
			get
			{
				string text = GenLabel.ThingLabel(this);
				for (int i = 0; i < this.comps.Count; i++)
				{
					text = this.comps[i].TransformLabel(text);
				}
				return text;
			}
		}

		public override void PostMake()
		{
			base.PostMake();
			this.InitializeComps();
		}

		public T GetComp<T>() where T : ThingComp
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				T t = this.comps[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}

		[DebuggerHidden]
		public IEnumerable<T> GetComps<T>() where T : ThingComp
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				T cT = this.comps[i] as T;
				if (cT != null)
				{
					yield return cT;
				}
			}
		}

		public ThingComp GetCompByDef(CompProperties def)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				if (this.comps[i].props == def)
				{
					return this.comps[i];
				}
			}
			return null;
		}

		public void InitializeComps()
		{
			for (int i = 0; i < this.def.comps.Count; i++)
			{
				ThingComp thingComp = (ThingComp)Activator.CreateInstance(this.def.comps[i].compClass);
				thingComp.parent = this;
				this.comps.Add(thingComp);
				thingComp.Initialize(this.def.comps[i]);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.InitializeComps();
			}
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostExposeData();
			}
		}

		public void BroadcastCompSignal(string signal)
		{
			this.ReceiveCompSignal(signal);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].ReceiveCompSignal(signal);
			}
		}

		protected virtual void ReceiveCompSignal(string signal)
		{
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostSpawnSetup();
			}
		}

		public override void DeSpawn()
		{
			Map map = base.Map;
			base.DeSpawn();
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostDeSpawn(map);
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.Destroy(mode);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostDestroy(mode, map);
			}
		}

		public override void Tick()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].CompTick();
			}
		}

		public override void TickRare()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].CompTickRare();
			}
		}

		public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(dinfo, out absorbed);
			if (absorbed)
			{
				return;
			}
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostPreApplyDamage(dinfo, out absorbed);
				if (absorbed)
				{
					return;
				}
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostApplyDamage(dinfo, totalDamageDealt);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostPostApplyDamage(dinfo, totalDamageDealt);
			}
		}

		public override void Draw()
		{
			base.Draw();
			this.Comps_PostDraw();
		}

		protected void Comps_PostDraw()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostDraw();
			}
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostDrawExtraSelectionOverlays();
			}
		}

		public override void Print(SectionLayer layer)
		{
			base.Print(layer);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostPrintOnto(layer);
			}
		}

		public virtual void PrintForPowerGrid(SectionLayer layer)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].CompPrintForPowerGrid(layer);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				foreach (Gizmo com in this.comps[i].CompGetGizmosExtra())
				{
					yield return com;
				}
			}
		}

		public override bool TryAbsorbStack(Thing other, bool respectStackLimit)
		{
			if (!this.CanStackWith(other))
			{
				return false;
			}
			int count = ThingUtility.TryAsborbStackNumToTake(this, other, respectStackLimit);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PreAbsorbStack(other, count);
			}
			return base.TryAbsorbStack(other, respectStackLimit);
		}

		public override Thing SplitOff(int count)
		{
			Thing thing = base.SplitOff(count);
			if (thing != null)
			{
				for (int i = 0; i < this.comps.Count; i++)
				{
					this.comps[i].PostSplitOff(thing);
				}
			}
			return thing;
		}

		public override bool CanStackWith(Thing other)
		{
			if (!base.CanStackWith(other))
			{
				return false;
			}
			for (int i = 0; i < this.comps.Count; i++)
			{
				if (!this.comps[i].AllowStackWith(other))
				{
					return false;
				}
			}
			return true;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.Append(this.InspectStringPartsFromComps());
			return stringBuilder.ToString();
		}

		protected string InspectStringPartsFromComps()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this.comps.Count; i++)
			{
				string text = this.comps[i].CompInspectStringExtra();
				if (!text.NullOrEmpty())
				{
					if (Prefs.DevMode && char.IsWhiteSpace(text[text.Length - 1]))
					{
						Log.ErrorOnce(this.comps[i].GetType() + " CompInspectStringExtra ended with whitespace: " + text, 25612);
						text = text.TrimEndNewlines();
					}
					stringBuilder.AppendLine(text);
				}
			}
			return stringBuilder.ToString();
		}

		public override string GetDescription()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetDescription());
			for (int i = 0; i < this.comps.Count; i++)
			{
				string descriptionPart = this.comps[i].GetDescriptionPart();
				if (!descriptionPart.NullOrEmpty())
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(descriptionPart);
				}
			}
			return stringBuilder.ToString();
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
		{
			foreach (FloatMenuOption o in base.GetFloatMenuOptions(selPawn))
			{
				yield return o;
			}
			for (int i = 0; i < this.comps.Count; i++)
			{
				foreach (FloatMenuOption o2 in this.comps[i].CompFloatMenuOptions(selPawn))
				{
					yield return o2;
				}
			}
		}

		public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PrePreTraded(action, playerNegotiator, trader);
			}
		}

		protected override void PostIngested(Pawn ingester)
		{
			base.PostIngested(ingester);
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].PostIngested(ingester);
			}
		}
	}
}
