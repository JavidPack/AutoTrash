﻿using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace AutoTrash
{
	internal class AutoTrash : Mod
	{
		// TODO: Pre-designed trash items sets. Sorting arrays?
		public Mod herosmod;
		public const string heropermission = "AutoTrash";
		public const string heropermissiondisplayname = "Auto Trash";
		public bool hasPermission;

		//public static AutoTrash instance;
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
			herosmod = ModLoader.GetMod("HEROsMod");
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

				UICheckbox.checkboxTexture = GetTexture("checkBox");
				UICheckbox.checkmarkTexture = GetTexture("checkMark");
			}
		}

		public AutoTrash GetInstance()
		{
			return instance;
		}

		public void SetupHerosMod()
		{
			if (herosmod != null)
			{
				herosmod.Call(
					// Special string
					"AddPermission",
					// Permission Name
					heropermission,
					// Permission Display Name
					heropermissiondisplayname);
			}
		}

		public bool getPermission()
		{
			return hasPermission;
		}

		public override void Unload()
		{
			instance = null;
			autoTrashUserInterface = null;
			herosmod = null;
			hasPermission = false;

			UICheckbox.checkboxTexture = null;
			UICheckbox.checkmarkTexture = null;
		}

		public override void PostAddRecipes()
		{
			SetupHerosMod();

		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryLayerIndex != -1)
			{
				layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash Slot",
					delegate
					{
						autoTrashGlobalItem.DrawUpdateAutoTrash();
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
							autoTrashUserInterface.Update(Main._drawInterfaceGameTime);
							autoTrashListUI.Draw(Main.spriteBatch);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		public override void PostSetupContent()
		{
			if (!Main.dedServ)
			{
				Mod RecipeBrowser = ModLoader.GetMod("RecipeBrowser");
				if (RecipeBrowser != null)
				{
					RecipeBrowser.Call("AddItemFilter", "Not Auto Trashed", "Weapons", GetTexture("RecipeBrowserFilterNotAutotrashedIcon"),
						(Predicate<Item>)((Item item) => !Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>().ShouldItemBeTrashed(item)));
				}
			}
		}
	}
}
