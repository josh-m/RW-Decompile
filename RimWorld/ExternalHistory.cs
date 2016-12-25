using System;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ExternalHistory : IExposable
	{
		public string gameVersion = "?";

		public string gameplayID = "?";

		public string userName = "?";

		public string storytellerName = "?";

		public string realWorldDate = "?";

		public string firstUploadDate = "?";

		public int firstUploadTime;

		public bool devMode;

		public History history = new History();

		public static string defaultUserName = "Anonymous";

		public string AllInformation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("storyteller: ");
				stringBuilder.Append(this.storytellerName);
				stringBuilder.Append("   userName: ");
				stringBuilder.Append(this.userName);
				stringBuilder.Append("   realWorldDate(UTC): ");
				stringBuilder.Append(this.realWorldDate);
				return stringBuilder.ToString();
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<string>(ref this.gameVersion, "gameVersion", null, false);
			Scribe_Values.LookValue<string>(ref this.gameplayID, "gameplayID", null, false);
			Scribe_Values.LookValue<string>(ref this.userName, "userName", null, false);
			Scribe_Values.LookValue<string>(ref this.storytellerName, "storytellerName", null, false);
			Scribe_Values.LookValue<string>(ref this.realWorldDate, "realWorldDate", null, false);
			Scribe_Values.LookValue<string>(ref this.firstUploadDate, "firstUploadDate", null, false);
			Scribe_Values.LookValue<int>(ref this.firstUploadTime, "firstUploadTime", 0, false);
			Scribe_Values.LookValue<bool>(ref this.devMode, "devMode", false, false);
			Scribe_Deep.LookDeep<History>(ref this.history, "history", new object[0]);
		}
	}
}
