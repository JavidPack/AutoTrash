using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria;
using Terraria.UI;

namespace AutoTrash
{
	internal class AutoTrash : Mod
	{
		internal static AutoTrash instance;
		internal AutoTrashGlobalItem autoTrashGlobalItem;
		internal static UserInterface autoTrashUserInterface;
		internal AutoTrashListUI autoTrashListUI;

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
			if (!Main.dedServ)
			{
				autoTrashListUI = new AutoTrashListUI();
				autoTrashListUI.Activate();
				autoTrashUserInterface = new UserInterface();
				autoTrashUserInterface.SetState(autoTrashListUI);
			}
		}

		public override void ModifyInterfaceLayers(List<MethodSequenceListItem> layers)
		{
			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryLayerIndex != -1)
			{
				layers.Insert(inventoryLayerIndex, new MethodSequenceListItem(
					"AutoTrash: Auto Trash Slot",
					delegate
					{
						autoTrashGlobalItem.DrawUpdateAutoTrash();
						return true;
					},
					null)
				);
			}

			int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (MouseTextIndex != -1)
			{
				layers.Insert(MouseTextIndex, new MethodSequenceListItem(
					"AutoTrash: Auto Trash List",
					delegate
					{
						if (AutoTrashListUI.visible)
						{
							autoTrashUserInterface.Update(Main._drawInterfaceGameTime);
							autoTrashListUI.Draw(Main.spriteBatch);
						}
						return true;
					},
					null)
				);
			}
		}
	}
}
