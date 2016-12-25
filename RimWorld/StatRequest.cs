using System;
using Verse;

namespace RimWorld
{
	public struct StatRequest : IEquatable<StatRequest>
	{
		private Thing thingInt;

		private BuildableDef defInt;

		private ThingDef stuffDefInt;

		public Thing Thing
		{
			get
			{
				return this.thingInt;
			}
		}

		public BuildableDef Def
		{
			get
			{
				return this.defInt;
			}
		}

		public ThingDef StuffDef
		{
			get
			{
				return this.stuffDefInt;
			}
		}

		public bool HasThing
		{
			get
			{
				return this.Thing != null;
			}
		}

		public bool Empty
		{
			get
			{
				return this.Def == null;
			}
		}

		public static StatRequest For(Thing thing)
		{
			if (thing == null)
			{
				Log.Error("StatRequest for null thing.");
				return StatRequest.ForEmpty();
			}
			return new StatRequest
			{
				thingInt = thing,
				defInt = thing.def,
				stuffDefInt = thing.Stuff
			};
		}

		public static StatRequest For(BuildableDef def, ThingDef stuffDef)
		{
			if (def == null)
			{
				Log.Error("StatRequest for null def.");
				return StatRequest.ForEmpty();
			}
			return new StatRequest
			{
				thingInt = null,
				defInt = def,
				stuffDefInt = stuffDef
			};
		}

		public static StatRequest ForEmpty()
		{
			return new StatRequest
			{
				thingInt = null,
				defInt = null,
				stuffDefInt = null
			};
		}

		public override string ToString()
		{
			if (this.Thing != null)
			{
				return "(" + this.Thing + ")";
			}
			return string.Concat(new object[]
			{
				"(",
				this.Thing,
				", ",
				(this.StuffDef == null) ? "null" : this.StuffDef.defName,
				")"
			});
		}

		public override int GetHashCode()
		{
			int num = 0;
			num = Gen.HashCombineInt(num, (int)this.defInt.shortHash);
			if (this.thingInt != null)
			{
				num = Gen.HashCombineInt(num, this.thingInt.thingIDNumber);
			}
			if (this.stuffDefInt != null)
			{
				num = Gen.HashCombineInt(num, (int)this.stuffDefInt.shortHash);
			}
			return num;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is StatRequest))
			{
				return false;
			}
			StatRequest statRequest = (StatRequest)obj;
			return statRequest.defInt == this.defInt && statRequest.thingInt == this.thingInt && statRequest.stuffDefInt == this.stuffDefInt;
		}

		public bool Equals(StatRequest other)
		{
			return other.defInt == this.defInt && other.thingInt == this.thingInt && other.stuffDefInt == this.stuffDefInt;
		}

		public static bool operator ==(StatRequest lhs, StatRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(StatRequest lhs, StatRequest rhs)
		{
			return !(lhs == rhs);
		}
	}
}
