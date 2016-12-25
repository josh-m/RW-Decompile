using System;

namespace RimWorld
{
	public static class MonthUtility
	{
		public static Season GetSeason(this Month month)
		{
			switch (month)
			{
			case Month.Jan:
				return Season.Winter;
			case Month.Feb:
				return Season.Winter;
			case Month.Mar:
				return Season.Spring;
			case Month.Apr:
				return Season.Spring;
			case Month.May:
				return Season.Spring;
			case Month.Jun:
				return Season.Summer;
			case Month.Jul:
				return Season.Summer;
			case Month.Aug:
				return Season.Summer;
			case Month.Sept:
				return Season.Fall;
			case Month.Oct:
				return Season.Fall;
			case Month.Nov:
				return Season.Fall;
			case Month.Dec:
				return Season.Winter;
			default:
				return Season.Undefined;
			}
		}

		public static Month NextMonth(this Month month)
		{
			return (month + 1) % Month.Undefined;
		}
	}
}
