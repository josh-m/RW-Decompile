using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Need_RoomSize : Need_Seeker
	{
		private static List<Room> tempScanRooms = new List<Room>();

		private const float MinCramped = 0.01f;

		private const float MinNormal = 0.3f;

		private const float MinSpacious = 0.7f;

		public static readonly int SampleNumCells = GenRadial.NumCellsInRadius(7.9f);

		private static readonly SimpleCurve RoomCellCountSpaceCurve = new SimpleCurve
		{
			{
				new CurvePoint(3f, 0f),
				true
			},
			{
				new CurvePoint(9f, 0.25f),
				true
			},
			{
				new CurvePoint(16f, 0.5f),
				true
			},
			{
				new CurvePoint(42f, 0.71f),
				true
			},
			{
				new CurvePoint(100f, 1f),
				true
			}
		};

		public override float CurInstantLevel
		{
			get
			{
				return this.SpacePerceptibleNow();
			}
		}

		public RoomSizeCategory CurCategory
		{
			get
			{
				if (this.CurLevel < 0.01f)
				{
					return RoomSizeCategory.VeryCramped;
				}
				if (this.CurLevel < 0.3f)
				{
					return RoomSizeCategory.Cramped;
				}
				if (this.CurLevel < 0.7f)
				{
					return RoomSizeCategory.Normal;
				}
				return RoomSizeCategory.Spacious;
			}
		}

		public Need_RoomSize(Pawn pawn) : base(pawn)
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
			Need_RoomSize.tempScanRooms.Clear();
			for (int i = 0; i < 5; i++)
			{
				IntVec3 loc = position + GenRadial.RadialPattern[i];
				Room room = loc.GetRoom(this.pawn.Map, RegionType.Set_Passable);
				if (room != null)
				{
					if (i == 0 && room.PsychologicallyOutdoors)
					{
						return 1f;
					}
					if (i == 0 || room.RegionType != RegionType.Portal)
					{
						if (!Need_RoomSize.tempScanRooms.Contains(room))
						{
							Need_RoomSize.tempScanRooms.Add(room);
						}
					}
				}
			}
			float num = 0f;
			for (int j = 0; j < Need_RoomSize.SampleNumCells; j++)
			{
				IntVec3 loc2 = position + GenRadial.RadialPattern[j];
				if (Need_RoomSize.tempScanRooms.Contains(loc2.GetRoom(this.pawn.Map, RegionType.Set_Passable)))
				{
					num += 1f;
				}
			}
			Need_RoomSize.tempScanRooms.Clear();
			return Need_RoomSize.RoomCellCountSpaceCurve.Evaluate(num);
		}
	}
}
