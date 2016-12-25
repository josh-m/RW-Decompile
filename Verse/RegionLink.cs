using System;
using System.Linq;

namespace Verse
{
	public class RegionLink
	{
		public Region[] regions = new Region[2];

		public EdgeSpan span;

		public Region RegionA
		{
			get
			{
				return this.regions[0];
			}
			set
			{
				this.regions[0] = value;
			}
		}

		public Region RegionB
		{
			get
			{
				return this.regions[1];
			}
			set
			{
				this.regions[1] = value;
			}
		}

		public void Register(Region reg)
		{
			if (this.regions[0] == reg || this.regions[1] == reg)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to double-register region ",
					reg.ToString(),
					" in ",
					this
				}));
				return;
			}
			if (this.RegionA == null || !this.RegionA.valid)
			{
				this.RegionA = reg;
			}
			else if (this.RegionB == null || !this.RegionB.valid)
			{
				this.RegionB = reg;
			}
			else
			{
				Log.Error(string.Concat(new object[]
				{
					"Could not register region ",
					reg.ToString(),
					" in link ",
					this,
					": > 2 regions on link!\nRegionA: ",
					this.RegionA.DebugString,
					"\nRegionB: ",
					this.RegionB.DebugString
				}));
			}
		}

		public void Deregister(Region reg)
		{
			if (this.RegionA == reg)
			{
				this.RegionA = null;
				if (this.RegionB == null)
				{
					reg.Map.regionLinkDatabase.Notify_LinkHasNoRegions(this);
				}
			}
			else if (this.RegionB == reg)
			{
				this.RegionB = null;
				if (this.RegionA == null)
				{
					reg.Map.regionLinkDatabase.Notify_LinkHasNoRegions(this);
				}
			}
		}

		public ulong UniqueHashCode()
		{
			return this.span.UniqueHashCode();
		}

		public override string ToString()
		{
			string text = GenText.ToCommaList(from r in this.regions
			where r != null
			select r.id.ToString(), true);
			string text2 = string.Concat(new object[]
			{
				"span=",
				this.span.ToString(),
				" hash=",
				this.UniqueHashCode()
			});
			return string.Concat(new string[]
			{
				"(",
				text2,
				", regions=",
				text,
				")"
			});
		}
	}
}
