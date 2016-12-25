using System;

namespace Verse
{
	public class Dialog_RenameArea : Dialog_Rename
	{
		private Area area;

		public Dialog_RenameArea(Area area)
		{
			this.area = area;
			this.curName = area.Label;
		}

		protected override AcceptanceReport NameIsValid(string name)
		{
			AcceptanceReport result = base.NameIsValid(name);
			if (!result.Accepted)
			{
				return result;
			}
			if (this.area.Map.areaManager.AllAreas.Any((Area a) => a.Label == name))
			{
				return "NameIsInUse".Translate();
			}
			return true;
		}

		protected override void SetName(string name)
		{
			this.area.SetLabel(this.curName);
		}
	}
}
