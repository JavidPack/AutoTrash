using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AutoTrash
{
	internal class AutoTrash : Mod
	{
		// TODO: Pre-designed trash items sets. Sorting arrays?
		internal static AutoTrash instance;
		//internal AutoTrashGlobalItem autoTrashGlobalItem;
		internal static UserInterface autoTrashUserInterface;
		internal AutoTrashListUI autoTrashListUI;

		public AutoTrash()
		{
		}

		public override void Load()
		{
			//if (ModLoader.version < new Version(0, 9, 1))
			//{
			//	throw new Exception("\nThis mod uses functionality only present in the latest tModLoader. Please update tModLoader to use this mod\n\n");
			//}
			Mod cheatSheet;
			ModLoader.TryGetMod("CheatSheet", out cheatSheet);
			if (cheatSheet != null && cheatSheet.Version <= new Version(0, 2, 5, 10))
			{
				throw new Exception("Please update CheatSheet to the latest version to use alongside AutoTrash");
			}
			instance = this;
			//autoTrashGlobalItem = (AutoTrashGlobalItem)GetGlobalItem("AutoTrashGlobalItem");
			if (!Main.dedServ)
			{
				autoTrashListUI = new AutoTrashListUI();
				autoTrashListUI.Activate();
				autoTrashUserInterface = new UserInterface();
				autoTrashUserInterface.SetState(autoTrashListUI);

				UICheckbox.checkboxTexture = Assets.Request<Texture2D>("checkBox", AssetRequestMode.ImmediateLoad);
				UICheckbox.checkmarkTexture = Assets.Request<Texture2D>("checkMark", AssetRequestMode.ImmediateLoad);
			}
		}

		public override void Unload()
		{
			instance = null;
			autoTrashUserInterface = null;

			UICheckbox.checkboxTexture = null;
			UICheckbox.checkmarkTexture = null;
		}

		public override void PostSetupContent()
		{
			/*
			if (!Main.dedServ)
			{
				Mod RecipeBrowser = ModLoader.GetMod("RecipeBrowser");
				if (RecipeBrowser != null)
				{
					RecipeBrowser.Call("AddItemFilter", "Not Auto Trashed", "Weapons", GetTexture("RecipeBrowserFilterNotAutotrashedIcon"),
						(Predicate<Item>)((Item item) => !Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>().ShouldItemBeTrashed(item)));
				}
			}
			*/
		}
	}

	internal class AutoTrashSystem : ModSystem
    {
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryLayerIndex != -1)
			{
				layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash Slot",
					delegate
					{
					 	ModContent.GetInstance<AutoTrashGlobalItem>().DrawUpdateAutoTrash();
						return true;
					},
					InterfaceScaleType.UI)
				);

				layers.Insert(inventoryLayerIndex + 2, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash Cursor",
					delegate
					{
						if (Main.cursorOverride == 6 && (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl)))
						{
							var autoTrashPlayer = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();
							if (autoTrashPlayer.AutoTrashEnabled)
								Main.cursorOverride = 5;
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}

			int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (MouseTextIndex != -1)
			{
				layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash List",
					delegate
					{
						if (AutoTrashListUI.visible)
						{
							AutoTrash.autoTrashUserInterface.Update(Main._drawInterfaceGameTime);
							ModContent.GetInstance<AutoTrash>().autoTrashListUI.Draw(Main.spriteBatch);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
