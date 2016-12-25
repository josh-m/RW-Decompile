using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class MoteMaker
	{
		public static Mote ThrowMetaIcon(IntVec3 cell, ThingDef moteDef)
		{
			if (!cell.ShouldSpawnMotesAt() || MoteCounter.Saturated)
			{
				return null;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(moteDef, null);
			moteThrown.Scale = 0.7f;
			moteThrown.rotationRate = Rand.Range(-3f, 3f);
			moteThrown.exactPosition = cell.ToVector3Shifted();
			moteThrown.exactPosition += new Vector3(0.35f, 0f, 0.35f);
			moteThrown.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value) * 0.1f;
			moteThrown.SetVelocity((float)Rand.Range(30, 60), 0.42f);
			GenSpawn.Spawn(moteThrown, cell);
			return moteThrown;
		}

		public static void MakeStaticMote(IntVec3 cell, ThingDef moteDef, float scale = 1f)
		{
			MoteMaker.MakeStaticMote(cell.ToVector3Shifted(), moteDef, scale);
		}

		public static void MakeStaticMote(Vector3 loc, ThingDef moteDef, float scale = 1f)
		{
			if (!loc.ShouldSpawnMotesAt() || MoteCounter.Saturated)
			{
				return;
			}
			Mote mote = (Mote)ThingMaker.MakeThing(moteDef, null);
			mote.exactPosition = loc;
			mote.Scale = scale;
			GenSpawn.Spawn(mote, loc.ToIntVec3());
		}

		public static void ThrowText(Vector3 loc, string text, float timeBeforeStartFadeout = -1f)
		{
			MoteMaker.ThrowText(loc, text, Color.white, timeBeforeStartFadeout);
		}

		public static void ThrowText(Vector3 loc, string text, Color color, float timeBeforeStartFadeout = -1f)
		{
			IntVec3 intVec = loc.ToIntVec3();
			if (!intVec.InBounds())
			{
				return;
			}
			MoteText moteText = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text, null);
			moteText.exactPosition = loc;
			moteText.SetVelocity((float)Rand.Range(5, 35), Rand.Range(0.42f, 0.45f));
			moteText.text = text;
			moteText.textColor = color;
			if (timeBeforeStartFadeout >= 0f)
			{
				moteText.overrideTimeBeforeStartFadeout = timeBeforeStartFadeout;
			}
			GenSpawn.Spawn(moteText, intVec);
		}

		public static void ThrowMetaPuffs(CellRect rect)
		{
			if (!Find.TickManager.Paused)
			{
				for (int i = rect.minX; i <= rect.maxX; i++)
				{
					for (int j = rect.minZ; j <= rect.maxZ; j++)
					{
						MoteMaker.ThrowMetaPuffs(new IntVec3(i, 0, j));
					}
				}
			}
		}

		public static void ThrowMetaPuffs(TargetInfo targ)
		{
			Vector3 a = (!targ.HasThing) ? targ.Cell.ToVector3Shifted() : targ.Thing.TrueCenter();
			int num = Rand.RangeInclusive(4, 6);
			for (int i = 0; i < num; i++)
			{
				Vector3 loc = a + new Vector3(Rand.Range(-0.5f, 0.5f), 0f, Rand.Range(-0.5f, 0.5f));
				MoteMaker.ThrowMetaPuff(loc);
			}
		}

		public static void ThrowMetaPuff(Vector3 loc)
		{
			if (!loc.ShouldSpawnMotesAt())
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_MetaPuff, null);
			moteThrown.Scale = 1.9f;
			moteThrown.rotationRate = (float)Rand.Range(-60, 60);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), Rand.Range(0.6f, 0.78f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}

		private static MoteThrown NewBaseAirPuff()
		{
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_AirPuff, null);
			moteThrown.Scale = 1.5f;
			moteThrown.rotationRate = (float)Rand.RangeInclusive(-240, 240);
			return moteThrown;
		}

		public static void ThrowAirPuffUp(Vector3 loc)
		{
			if (!loc.ToIntVec3().ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = MoteMaker.NewBaseAirPuff();
			moteThrown.exactPosition = loc;
			moteThrown.exactPosition += new Vector3(Rand.Range(-0.02f, 0.02f), 0f, Rand.Range(-0.02f, 0.02f));
			moteThrown.SetVelocity((float)Rand.Range(-45, 45), Rand.Range(1.2f, 1.5f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}

		internal static void ThrowBreathPuff(Vector3 loc, float throwAngle, Vector3 inheritVelocity)
		{
			if (!loc.ToIntVec3().ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = MoteMaker.NewBaseAirPuff();
			moteThrown.exactPosition = loc;
			moteThrown.exactPosition += new Vector3(Rand.Range(-0.005f, 0.005f), 0f, Rand.Range(-0.005f, 0.005f));
			moteThrown.SetVelocity(throwAngle + (float)Rand.Range(-10, 10), Rand.Range(0.1f, 0.8f));
			moteThrown.Velocity += inheritVelocity * 0.5f;
			moteThrown.Scale = Rand.Range(0.6f, 0.7f);
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}

		public static void ThrowSmoke(Vector3 loc, float size)
		{
			if (!loc.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke, null);
			moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
			moteThrown.rotationRate = Rand.Range(-30f, 30f);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}

		public static void ThrowFireGlow(IntVec3 c, float size)
		{
			Vector3 vector = c.ToVector3Shifted();
			if (!vector.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
			if (!vector.InBounds())
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_FireGlow, null);
			moteThrown.Scale = Rand.Range(4f, 6f) * size;
			moteThrown.rotationRate = Rand.Range(-3f, 3f);
			moteThrown.exactPosition = vector;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), 0.12f);
			GenSpawn.Spawn(moteThrown, vector.ToIntVec3());
		}

		public static void ThrowHeatGlow(IntVec3 c, float size)
		{
			Vector3 vector = c.ToVector3Shifted();
			if (!vector.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
			if (!vector.InBounds())
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_HeatGlow, null);
			moteThrown.Scale = Rand.Range(4f, 6f) * size;
			moteThrown.rotationRate = Rand.Range(-3f, 3f);
			moteThrown.exactPosition = vector;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), 0.12f);
			GenSpawn.Spawn(moteThrown, vector.ToIntVec3());
		}

		public static void ThrowMicroSparks(Vector3 loc)
		{
			if (!loc.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_MicroSparks, null);
			moteThrown.Scale = Rand.Range(0.8f, 1.2f);
			moteThrown.rotationRate = Rand.Range(-12f, 12f);
			moteThrown.exactPosition = loc;
			moteThrown.exactPosition -= new Vector3(0.5f, 0f, 0.5f);
			moteThrown.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value);
			moteThrown.SetVelocity((float)Rand.Range(35, 45), 1.2f);
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}

		public static void ThrowLightningGlow(Vector3 loc, float size)
		{
			if (!loc.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_LightningGlow, null);
			moteThrown.Scale = Rand.Range(4f, 6f) * size;
			moteThrown.rotationRate = Rand.Range(-3f, 3f);
			moteThrown.exactPosition = loc + size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
			moteThrown.SetVelocity((float)Rand.Range(0, 360), 1.2f);
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}

		public static void PlaceFootprint(Vector3 loc, float rot)
		{
			if (!loc.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Footprint, null);
			moteThrown.Scale = 0.5f;
			moteThrown.exactRotation = rot;
			moteThrown.exactPosition = loc;
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}

		public static void ThrowHorseshoe(Pawn thrower, IntVec3 targetCell)
		{
			if (!thrower.Position.ShouldSpawnMotesAt() || MoteCounter.Saturated)
			{
				return;
			}
			float num = Rand.Range(3.8f, 5.6f);
			Vector3 vector = targetCell.ToVector3Shifted() + Vector3Utility.RandomHorizontalOffset((1f - (float)thrower.skills.GetSkill(SkillDefOf.Shooting).level / 20f) * 1.8f);
			vector.y = thrower.DrawPos.y;
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Horseshoe, null);
			moteThrown.Scale = 1f;
			moteThrown.rotationRate = (float)Rand.Range(-300, 300);
			moteThrown.exactPosition = thrower.DrawPos;
			moteThrown.SetVelocity((vector - moteThrown.exactPosition).AngleFlat(), num);
			moteThrown.airTimeLeft = (float)Mathf.RoundToInt((moteThrown.exactPosition - vector).MagnitudeHorizontal() / num);
			GenSpawn.Spawn(moteThrown, thrower.Position);
		}

		public static Mote MakeStunOverlay(Thing stunnedThing)
		{
			Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_Stun, null);
			mote.Attach(stunnedThing);
			GenSpawn.Spawn(mote, stunnedThing.Position);
			return mote;
		}

		public static void MakeInteractionMote(Pawn initiator, Pawn recipient, ThingDef interactionMote, Texture2D symbol)
		{
			MoteInteraction moteInteraction = (MoteInteraction)ThingMaker.MakeThing(interactionMote, null);
			moteInteraction.Scale = 1.25f;
			moteInteraction.SetupInteractionMote(symbol, recipient);
			moteInteraction.Attach(initiator);
			GenSpawn.Spawn(moteInteraction, initiator.Position);
		}

		public static MoteDualAttached MakeInteractionOverlay(ThingDef moteDef, TargetInfo A, TargetInfo B)
		{
			MoteDualAttached moteDualAttached = (MoteDualAttached)ThingMaker.MakeThing(moteDef, null);
			moteDualAttached.Scale = 0.5f;
			moteDualAttached.Attach(A, B);
			GenSpawn.Spawn(moteDualAttached, A.Cell);
			return moteDualAttached;
		}

		public static void MakeColonistActionOverlay(Pawn pawn, ThingDef moteDef)
		{
			MoteThrownAttached moteThrownAttached = (MoteThrownAttached)ThingMaker.MakeThing(moteDef, null);
			moteThrownAttached.Attach(pawn);
			moteThrownAttached.Scale = 1.5f;
			moteThrownAttached.SetVelocity(Rand.Range(20f, 25f), 0.4f);
			GenSpawn.Spawn(moteThrownAttached, pawn.Position);
		}

		public static void ThrowDustPuff(IntVec3 cell, float scale)
		{
			Vector3 loc = cell.ToVector3() + new Vector3(Rand.Value, 0f, Rand.Value);
			MoteMaker.ThrowDustPuff(loc, scale);
		}

		public static void ThrowDustPuff(Vector3 loc, float scale)
		{
			if (!loc.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_DustPuff, null);
			moteThrown.Scale = 1.9f * scale;
			moteThrown.rotationRate = (float)Rand.Range(-60, 60);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}

		public static void PlaceTempRoof(IntVec3 cell)
		{
			if (!cell.ShouldSpawnMotesAt())
			{
				return;
			}
			Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_TempRoof, null);
			mote.exactPosition = cell.ToVector3Shifted();
			GenSpawn.Spawn(mote, cell);
		}

		public static void ThrowExplosionCell(IntVec3 cell, ThingDef moteDef, Color color)
		{
			if (!cell.ShouldSpawnMotesAt())
			{
				return;
			}
			Mote mote = (Mote)ThingMaker.MakeThing(moteDef, null);
			mote.exactRotation = (float)(90 * Rand.RangeInclusive(0, 3));
			mote.exactPosition = cell.ToVector3Shifted();
			mote.instanceColor = color;
			GenSpawn.Spawn(mote, cell);
			if (Rand.Value < 0.7f)
			{
				MoteMaker.ThrowDustPuff(cell, 1.2f);
			}
		}

		public static void ThrowExplosionInteriorMote(Vector3 loc, ThingDef moteDef)
		{
			if (!loc.ShouldSpawnMotesAt() || MoteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(moteDef, null);
			moteThrown.Scale = Rand.Range(3f, 4.5f);
			moteThrown.rotationRate = Rand.Range(-30f, 30f);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), Rand.Range(0.48f, 0.72f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
		}
	}
}
