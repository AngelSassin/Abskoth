using System.Collections.Generic;
using Modding;

namespace Abskoth
{
	public partial class Abskoth : IMenuMod
	{
		public bool ToggleButtonInsideMenu => true;

		public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
		{
			List<IMenuMod.MenuEntry> entries = new List<IMenuMod.MenuEntry>();

			entries.Add((IMenuMod.MenuEntry)toggleButtonEntry);
			
			entries.Add(new IMenuMod.MenuEntry
			{
				Name = "Number of Markoths",
				Description = "# of Markoths in the Absrad fight. This changes nothing when mid-fight.",
				Values = new string[] { "1", "2", "3", "4" },
				Saver = (i) => GlobalSaveData.numMarkoths = i + 1,
				Loader = () => GlobalSaveData.numMarkoths > 0 ? GlobalSaveData.numMarkoths - 1 : 0
			});

			entries.Add(new IMenuMod.MenuEntry
			{
				Name = "Spawn Markoth's Nail Shots",
				Description = "Toggle whether Markoth can spawn Nail Shots in the fight.",
				Values = new string[] { "On", "Off" },
				Saver = (i) => GlobalSaveData.withNail = i == 0,
				Loader = () => GlobalSaveData.withNail ? 0 : 1
			});

			return entries;
		}
	}
}
