using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace AutoTrash
{
	internal class AutoTrash : Mod
	{
		// TODO: Pre-designed trash items sets. Sorting arrays?
		internal static AutoTrash instance;
		internal static UserInterface autoTrashUserInterface;
		internal AutoTrashListUI autoTrashListUI;

		public override void Load() {
			instance = this;
		}

		public override void Unload() {
			instance = null;
			autoTrashUserInterface = null;

			UICheckbox.checkboxTexture = null;
			UICheckbox.checkmarkTexture = null;
		}

		public override void PostSetupContent() {
			if (!Main.dedServ)
			{
				autoTrashListUI = new AutoTrashListUI();
				autoTrashListUI.Activate();
				autoTrashUserInterface = new UserInterface();
				autoTrashUserInterface.SetState(autoTrashListUI);

				UICheckbox.checkboxTexture = Assets.Request<Texture2D>("checkBox", AssetRequestMode.ImmediateLoad);
				UICheckbox.checkmarkTexture = Assets.Request<Texture2D>("checkMark", AssetRequestMode.ImmediateLoad);

				if (ModLoader.TryGetMod("RecipeBrowser", out Mod RecipeBrowser))
				{
					RecipeBrowser.Call("AddItemFilter", Language.GetTextValue("Mods.AutoTrash.RecipeBrowserFilterNotAutoTrashed"), "Weapons", Assets.Request<Texture2D>("RecipeBrowserFilterNotAutotrashedIcon", AssetRequestMode.ImmediateLoad).Value,
						(Predicate<Item>)((Item item) => !Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>().ShouldItemBeTrashed(item)));

					// TODO: Update RecipeBrowser with LocalizedText support, maybe some way of triggering a refresh.
				}
			}
		}
	}
}
