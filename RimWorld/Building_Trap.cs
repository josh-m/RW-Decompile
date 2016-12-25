using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Building_Trap : Building
	{
		private const float KnowerSpringChance = 0.004f;

		private const ushort KnowerPathFindCost = 800;

		private const ushort KnowerPathWalkCost = 30;

		private const float AnimalSpringChanceFactor = 0.1f;

		private List<Pawn> touchingPawns = new List<Pawn>();

		public virtual bool Armed
		{
			get
			{
				return true;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.LookList<Pawn>(ref this.touchingPawns, "testees", LookMode.Reference, new object[0]);
		}

		public override void Tick()
		{
			if (this.Armed)
			{
				List<Thing> thingList = base.Position.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null && !this.touchingPawns.Contains(pawn))
					{
						this.touchingPawns.Add(pawn);
						this.CheckSpring(pawn);
					}
				}
			}
			for (int j = 0; j < this.touchingPawns.Count; j++)
			{
				Pawn pawn2 = this.touchingPawns[j];
				if (!pawn2.Spawned || pawn2.Position != base.Position)
				{
					this.touchingPawns.Remove(pawn2);
				}
			}
			base.Tick();
		}

		protected virtual float SpringChance(Pawn p)
		{
			float num;
			if (this.KnowsOfTrap(p))
			{
				num = 0.004f;
			}
			else
			{
				num = this.GetStatValue(StatDefOf.TrapSpringChance, true);
			}
			num *= GenMath.LerpDouble(0.4f, 0.8f, 0f, 1f, p.BodySize);
			if (p.RaceProps.Animal)
			{
				num *= 0.1f;
			}
			return Mathf.Clamp01(num);
		}

		private void CheckSpring(Pawn p)
		{
			if (Rand.Value < this.SpringChance(p))
			{
				this.Spring(p);
				if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
				{
					Letter let = new Letter("LetterFriendlyTrapSprungLabel".Translate(new object[]
					{
						p.NameStringShort
					}), "LetterFriendlyTrapSprung".Translate(new object[]
					{
						p.NameStringShort
					}), LetterType.BadNonUrgent, new TargetInfo(base.Position, base.Map, false));
					Find.LetterStack.ReceiveLetter(let, null);
				}
			}
		}

		public bool KnowsOfTrap(Pawn p)
		{
			return (p.Faction != null && !p.Faction.HostileTo(base.Faction)) || (p.Faction == null && p.RaceProps.Animal && !p.InAggroMentalState) || (p.guest != null && p.guest.released);
		}

		public override ushort PathFindCostFor(Pawn p)
		{
			if (!this.Armed)
			{
				return 0;
			}
			if (this.KnowsOfTrap(p))
			{
				return 800;
			}
			return 0;
		}

		public override ushort PathWalkCostFor(Pawn p)
		{
			if (!this.Armed)
			{
				return 0;
			}
			if (this.KnowsOfTrap(p))
			{
				return 30;
			}
			return 0;
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			text += "\n";
			if (this.Armed)
			{
				text += "TrapArmed".Translate();
			}
			else
			{
				text += "TrapNotArmed".Translate();
			}
			return text;
		}

		private void Spring(Pawn p)
		{
			SoundDef.Named("DeadfallSpring").PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			if (p.Faction != null)
			{
				p.Faction.TacticalMemory.TrapRevealed(base.Position, base.Map);
			}
			this.SpringSub(p);
		}

		protected abstract void SpringSub(Pawn p);
	}
}
