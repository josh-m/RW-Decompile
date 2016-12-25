using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Need_Space : Need_Seeker
	{
		private const float MinCramped = 0.01f;

		private const float MinNormal = 0.3f;

		private const float MinSpacious = 0.7f;

		private static List<Room> tempScanRooms = new List<Room>();

		public static readonly int SampleNumCells = GenRadial.NumCellsInRadius(7.9f);

		private static readonly SimpleCurve RoomCellCountSpaceCurve = new SimpleCurve
		{
			new CurvePoint(3f, 0f),
			new CurvePoint(9f, 0.25f),
			new CurvePoint(16f, 0.5f),
			new CurvePoint(42f, 0.71f),
			new CurvePoint(100f, 1f)
		};

		public override float CurInstantLevel
		{
			get
			{
				return this.SpacePerceptibleNow();
			}
		}

		public SpaceCategory CurCategory
		{
			get
			{
				if (this.CurLevel < 0.01f)
				{
					return SpaceCategory.VeryCramped;
				}
				if (this.CurLevel < 0.3f)
				{
					return SpaceCategory.Cramped;
				}
				if (this.CurLevel < 0.7f)
				{
					return SpaceCategory.Normal;
				}
				return SpaceCategory.Spacious;
			}
		}

		public Need_Space(Pawn pawn) : base(pawn)
		{
			this.threshPercents = new List<float>();
			this.threshPercents.Add(0.3f);
			this.threshPercents.Add(0.7f);
		}

		public float SpacePerceptibleNow()
		{
			if (!this.pawn.Spawned)
			{
				return 1f;
			}
			IntVec3 position = this.pawn.Position;
			Need_Space.tempScanRooms.Clear();
			for (int i = 0; i < 5; i++)
			{
				IntVec3 loc = position + GenRadial.RadialPattern[i];
				Room room = loc.GetRoom(this.pawn.Map);
				if (room != null)
				{
					if (i == 0 && room.PsychologicallyOutdoors)
					{
						return 1f;
					}
					if (i == 0 || !room.IsDoor)
					{
						if (!Need_Space.tempScanRooms.Contains(room))
						{
							Need_Space.tempScanRooms.Add(room);
						}
					}
				}
			}
			float num = 0f;
			for (int j = 0; j < Need_Space.SampleNumCells; j++)
			{
				IntVec3 c = position + GenRadial.RadialPattern[j];
				if (Need_Space.tempScanRooms.Contains(RoomQuery.RoomAt(c, this.pawn.Map)))
				{
					num += 1f;
				}
			}
			Need_Space.tempScanRooms.Clear();
			return Need_Space.RoomCellCountSpaceCurve.Evaluate(num);
		}
	}
}
