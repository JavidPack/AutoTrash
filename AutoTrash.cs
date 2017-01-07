using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;

namespace AutoTrash
{
	internal class AutoTrash : Mod
	{
		internal static AutoTrash instance;
		internal AutoTrashGlobalItem autoTrashGlobalItem;

		public AutoTrash()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
			};
		}

		public override void Load()
		{
			//if (ModLoader.version < new Version(0, 9, 1))
			//{
			//	throw new Exception("\nThis mod uses functionality only present in the latest tModLoader. Please update tModLoader to use this mod\n\n");
			//}
			Mod cheatSheet = ModLoader.GetMod("CheatSheet");
			if (cheatSheet != null && cheatSheet.Version <= new Version(0, 2, 5, 10))
			{
				throw new Exception("Please update CheatSheet to the latest version to use alongside AutoTrash");
			}
			instance = this;
			autoTrashGlobalItem = (AutoTrashGlobalItem)GetGlobalItem("AutoTrashGlobalItem");
		}

		public override void ModifyInterfaceLayers(List<MethodSequenceListItem> layers)
		{
			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryLayerIndex != -1)
			{
				layers.Insert(inventoryLayerIndex, new MethodSequenceListItem(
					"AutoTrash: Auto Trash",
					delegate
					{
						autoTrashGlobalItem.DrawUpdateAutoTrash();
						return true;
					},
					null)
				);
			}
		}
	}
}
