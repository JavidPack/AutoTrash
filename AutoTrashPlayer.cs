using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace AutoTrash
{
	internal class AutoTrashPlayer : ModPlayer
	{
		public bool AutoTrashEnabled = false;
		public List<Item> AutoTrashItems;
		public Item LastAutoTrashItem;

		public override void Initialize()
		{
			AutoTrashItems = new List<Item>();
			LastAutoTrashItem = new Item();
			LastAutoTrashItem.SetDefaults();
			AutoTrashEnabled = false;
		}

		public override TagCompound Save()
		{
			return new TagCompound
			{
				["AutoTrashItems"] = AutoTrashItems.Select(ItemIO.Save).ToList(),
				["AutoTrashEnabled"] = AutoTrashEnabled
			};
		}

		public override void Load(TagCompound tag)
		{
			AutoTrashItems = tag.GetList<TagCompound>("AutoTrashItems").Select(ItemIO.Load).ToList();
			AutoTrashEnabled = tag.GetBool("AutoTrashEnabled");

			AutoTrash.instance.autoTrashListUI.UpdateNeeded();
		}

		internal static bool IsModItem(Item item)
		{
			return item.type >= ItemID.Count;
		}
	}
}
