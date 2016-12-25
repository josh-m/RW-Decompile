using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class MoteMaker
	{
		private static IntVec3[] UpRightPattern = new IntVec3[]
		{
			new IntVec3(0, 0, 0),
			new IntVec3(1, 0, 0),
			new IntVec3(0, 0, 1),
			new IntVec3(1, 0, 1)
		};

		public static Mote ThrowMetaIcon(IntVec3 cell, Map map, ThingDef moteDef)
		{
			if (!cell.ShouldSpawnMotesAt(map) || map.moteCounter.Saturated)
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
			GenSpawn.Spawn(moteThrown, cell, map);
			return moteThrown;
		}

		public static void MakeStaticMote(IntVec3 cell, Map map, ThingDef moteDef, float scale = 1f)
		{
			MoteMaker.MakeStaticMote(cell.ToVector3Shifted(), map, moteDef, scale);
		}

		public static void MakeStaticMote(Vector3 loc, Map map, ThingDef moteDef, float scale = 1f)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.Saturated)
			{
				return;
			}
			Mote mote = (Mote)ThingMaker.MakeThing(moteDef, null);
			mote.exactPosition = loc;
			mote.Scale = scale;
			GenSpawn.Spawn(mote, loc.ToIntVec3(), map);
		}

		public static void ThrowText(Vector3 loc, Map map, string text, float timeBeforeStartFadeout = -1f)
		{
			MoteMaker.ThrowText(loc, map, text, Color.white, timeBeforeStartFadeout);
		}

		public static void ThrowText(Vector3 loc, Map map, string text, Color color, float timeBeforeStartFadeout = -1f)
		{
			IntVec3 intVec = loc.ToIntVec3();
			if (!intVec.InBounds(map))
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
			GenSpawn.Spawn(moteText, intVec, map);
		}

		public static void ThrowMetaPuffs(CellRect rect, Map map)
		{
			if (!Find.TickManager.Paused)
			{
				for (int i = rect.minX; i <= rect.maxX; i++)
				{
					for (int j = rect.minZ; j <= rect.maxZ; j++)
					{
						MoteMaker.ThrowMetaPuffs(new TargetInfo(new IntVec3(i, 0, j), map, false));
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
				MoteMaker.ThrowMetaPuff(loc, targ.Map);
			}
		}

		public static void ThrowMetaPuff(Vector3 loc, Map map)
		{
			if (!loc.ShouldSpawnMotesAt(map))
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_MetaPuff, null);
			moteThrown.Scale = 1.9f;
			moteThrown.rotationRate = (float)Rand.Range(-60, 60);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), Rand.Range(0.6f, 0.78f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		private static MoteThrown NewBaseAirPuff()
		{
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_AirPuff, null);
			moteThrown.Scale = 1.5f;
			moteThrown.rotationRate = (float)Rand.RangeInclusive(-240, 240);
			return moteThrown;
		}

		public static void ThrowAirPuffUp(Vector3 loc, Map map)
		{
			if (!loc.ToIntVec3().ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = MoteMaker.NewBaseAirPuff();
			moteThrown.exactPosition = loc;
			moteThrown.exactPosition += new Vector3(Rand.Range(-0.02f, 0.02f), 0f, Rand.Range(-0.02f, 0.02f));
			moteThrown.SetVelocity((float)Rand.Range(-45, 45), Rand.Range(1.2f, 1.5f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		internal static void ThrowBreathPuff(Vector3 loc, Map map, float throwAngle, Vector3 inheritVelocity)
		{
			if (!loc.ToIntVec3().ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = MoteMaker.NewBaseAirPuff();
			moteThrown.exactPosition = loc;
			moteThrown.exactPosition += new Vector3(Rand.Range(-0.005f, 0.005f), 0f, Rand.Range(-0.005f, 0.005f));
			moteThrown.SetVelocity(throwAngle + (float)Rand.Range(-10, 10), Rand.Range(0.1f, 0.8f));
			moteThrown.Velocity += inheritVelocity * 0.5f;
			moteThrown.Scale = Rand.Range(0.6f, 0.7f);
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		public static void ThrowDustPuff(IntVec3 cell, Map map, float scale)
		{
			Vector3 loc = cell.ToVector3() + new Vector3(Rand.Value, 0f, Rand.Value);
			MoteMaker.ThrowDustPuff(loc, map, scale);
		}

		public static void ThrowDustPuff(Vector3 loc, Map map, float scale)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_DustPuff, null);
			moteThrown.Scale = 1.9f * scale;
			moteThrown.rotationRate = (float)Rand.Range(-60, 60);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), Rand.Range(0.6f, 0.75f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		public static void ThrowSmoke(Vector3 loc, Map map, float size)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke, null);
			moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
			moteThrown.rotationRate = Rand.Range(-30f, 30f);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		public static void ThrowFireGlow(IntVec3 c, Map map, float size)
		{
			Vector3 vector = c.ToVector3Shifted();
			if (!vector.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
			if (!vector.InBounds(map))
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_FireGlow, null);
			moteThrown.Scale = Rand.Range(4f, 6f) * size;
			moteThrown.rotationRate = Rand.Range(-3f, 3f);
			moteThrown.exactPosition = vector;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), 0.12f);
			GenSpawn.Spawn(moteThrown, vector.ToIntVec3(), map);
		}

		public static void ThrowHeatGlow(IntVec3 c, Map map, float size)
		{
			Vector3 vector = c.ToVector3Shifted();
			if (!vector.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			vector += size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
			if (!vector.InBounds(map))
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_HeatGlow, null);
			moteThrown.Scale = Rand.Range(4f, 6f) * size;
			moteThrown.rotationRate = Rand.Range(-3f, 3f);
			moteThrown.exactPosition = vector;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), 0.12f);
			GenSpawn.Spawn(moteThrown, vector.ToIntVec3(), map);
		}

		public static void ThrowMicroSparks(Vector3 loc, Map map)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
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
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		public static void ThrowLightningGlow(Vector3 loc, Map map, float size)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_LightningGlow, null);
			moteThrown.Scale = Rand.Range(4f, 6f) * size;
			moteThrown.rotationRate = Rand.Range(-3f, 3f);
			moteThrown.exactPosition = loc + size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
			moteThrown.SetVelocity((float)Rand.Range(0, 360), 1.2f);
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		public static void PlaceFootprint(Vector3 loc, Map map, float rot)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Footprint, null);
			moteThrown.Scale = 0.5f;
			moteThrown.exactRotation = rot;
			moteThrown.exactPosition = loc;
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		public static void ThrowHorseshoe(Pawn thrower, IntVec3 targetCell)
		{
			if (!thrower.Position.ShouldSpawnMotesAt(thrower.Map) || thrower.Map.moteCounter.Saturated)
			{
				return;
			}
			float num = Rand.Range(3.8f, 5.6f);
			Vector3 vector = targetCell.ToVector3Shifted() + Vector3Utility.RandomHorizontalOffset((1f - (float)thrower.skills.GetSkill(SkillDefOf.Shooting).Level / 20f) * 1.8f);
			vector.y = thrower.DrawPos.y;
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Horseshoe, null);
			moteThrown.Scale = 1f;
			moteThrown.rotationRate = (float)Rand.Range(-300, 300);
			moteThrown.exactPosition = thrower.DrawPos;
			moteThrown.SetVelocity((vector - moteThrown.exactPosition).AngleFlat(), num);
			moteThrown.airTimeLeft = (float)Mathf.RoundToInt((moteThrown.exactPosition - vector).MagnitudeHorizontal() / num);
			GenSpawn.Spawn(moteThrown, thrower.Position, thrower.Map);
		}

		public static Mote MakeStunOverlay(Thing stunnedThing)
		{
			Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_Stun, null);
			mote.Attach(stunnedThing);
			GenSpawn.Spawn(mote, stunnedThing.Position, stunnedThing.Map);
			return mote;
		}

		public static MoteDualAttached MakeInteractionOverlay(ThingDef moteDef, TargetInfo A, TargetInfo B)
		{
			MoteDualAttached moteDualAttached = (MoteDualAttached)ThingMaker.MakeThing(moteDef, null);
			moteDualAttached.Scale = 0.5f;
			moteDualAttached.Attach(A, B);
			GenSpawn.Spawn(moteDualAttached, A.Cell, A.Map ?? B.Map);
			return moteDualAttached;
		}

		public static void MakeColonistActionOverlay(Pawn pawn, ThingDef moteDef)
		{
			MoteThrownAttached moteThrownAttached = (MoteThrownAttached)ThingMaker.MakeThing(moteDef, null);
			moteThrownAttached.Attach(pawn);
			moteThrownAttached.Scale = 1.5f;
			moteThrownAttached.SetVelocity(Rand.Range(20f, 25f), 0.4f);
			GenSpawn.Spawn(moteThrownAttached, pawn.Position, pawn.Map);
		}

		private static MoteBubble ExistingMoteBubbleOn(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return null;
			}
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = pawn.Position + MoteMaker.UpRightPattern[i];
				if (c.InBounds(pawn.Map))
				{
					List<Thing> thingList = pawn.Position.GetThingList(pawn.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						Thing thing = thingList[j];
						MoteBubble moteBubble = thing as MoteBubble;
						if (moteBubble != null && moteBubble.link1.Linked && moteBubble.link1.Target.HasThing && moteBubble.link1.Target == pawn)
						{
							return moteBubble;
						}
					}
				}
			}
			return null;
		}

		public static MoteBubble MakeMoodThoughtBubble(Pawn pawn, Thought thought)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return null;
			}
			if (!pawn.Spawned)
			{
				return null;
			}
			float num = thought.MoodOffset();
			if (num == 0f)
			{
				return null;
			}
			MoteBubble moteBubble = MoteMaker.ExistingMoteBubbleOn(pawn);
			if (moteBubble != null)
			{
				if (moteBubble.def == ThingDefOf.Mote_Speech)
				{
					return null;
				}
				if (moteBubble.def == ThingDefOf.Mote_ThoughtBad || moteBubble.def == ThingDefOf.Mote_ThoughtGood)
				{
					moteBubble.Destroy(DestroyMode.Vanish);
				}
			}
			ThingDef def = (num <= 0f) ? ThingDefOf.Mote_ThoughtBad : ThingDefOf.Mote_ThoughtGood;
			MoteBubble moteBubble2 = (MoteBubble)ThingMaker.MakeThing(def, null);
			moteBubble2.SetupMoteBubble(thought.Icon, null);
			moteBubble2.Attach(pawn);
			GenSpawn.Spawn(moteBubble2, pawn.Position, pawn.Map);
			return moteBubble2;
		}

		public static MoteBubble MakeInteractionBubble(Pawn initiator, Pawn recipient, ThingDef interactionMote, Texture2D symbol)
		{
			MoteBubble moteBubble = MoteMaker.ExistingMoteBubbleOn(initiator);
			if (moteBubble != null)
			{
				if (moteBubble.def == ThingDefOf.Mote_Speech)
				{
					moteBubble.Destroy(DestroyMode.Vanish);
				}
				if (moteBubble.def == ThingDefOf.Mote_ThoughtBad || moteBubble.def == ThingDefOf.Mote_ThoughtGood)
				{
					moteBubble.Destroy(DestroyMode.Vanish);
				}
			}
			MoteBubble moteBubble2 = (MoteBubble)ThingMaker.MakeThing(interactionMote, null);
			moteBubble2.SetupMoteBubble(symbol, recipient);
			moteBubble2.Attach(initiator);
			GenSpawn.Spawn(moteBubble2, initiator.Position, initiator.Map);
			return moteBubble2;
		}

		public static void ThrowExplosionCell(IntVec3 cell, Map map, ThingDef moteDef, Color color)
		{
			if (!cell.ShouldSpawnMotesAt(map))
			{
				return;
			}
			Mote mote = (Mote)ThingMaker.MakeThing(moteDef, null);
			mote.exactRotation = (float)(90 * Rand.RangeInclusive(0, 3));
			mote.exactPosition = cell.ToVector3Shifted();
			mote.instanceColor = color;
			GenSpawn.Spawn(mote, cell, map);
			if (Rand.Value < 0.7f)
			{
				MoteMaker.ThrowDustPuff(cell, map, 1.2f);
			}
		}

		public static void ThrowExplosionInteriorMote(Vector3 loc, Map map, ThingDef moteDef)
		{
			if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(moteDef, null);
			moteThrown.Scale = Rand.Range(3f, 4.5f);
			moteThrown.rotationRate = Rand.Range(-30f, 30f);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(0, 360), Rand.Range(0.48f, 0.72f));
			GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
		}

		public static void PlaceTempRoof(IntVec3 cell, Map map)
		{
			if (!cell.ShouldSpawnMotesAt(map))
			{
				return;
			}
			Mote mote = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_TempRoof, null);
			mote.exactPosition = cell.ToVector3Shifted();
			GenSpawn.Spawn(mote, cell, map);
		}
	}
}
