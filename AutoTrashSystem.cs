using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace AutoTrash
{
	internal class AutoTrashSystem : ModSystem
	{
		public override void Load() {
			On_Item.NewItem_Inner += SkipSpawnIfAutoTrashed;

			// Applies on chest and quest rewards as well
			On_Player.GetItem += OnPickupV2;
		}

		private static Item OnPickupV2(On_Player.orig_GetItem orig, Player player, int plrIndex, Item item, GetItemSettings settings) {
			AutoTrashPlayer modPlayer = player.GetModPlayer<AutoTrashPlayer>();

			if (AutoTrashPlayer.IsModItem(item) && item.ModItem is Terraria.ModLoader.Default.UnloadedItem) {
				return orig(player, plrIndex, item, settings);
			}

			if (modPlayer.AutoTrashEnabled && modPlayer.ShouldItemBeTrashed(item)) {
				modPlayer.LastAutoTrashItem = item;
				modPlayer.OnItemAutotrashed();
				//Main.item[j] = player.GetItem(player.whoAmI, Main.item[j], false, false);
				return new Item();
			}

			return orig(player, plrIndex, item, settings);
		}

		private static int SkipSpawnIfAutoTrashed(On_Item.orig_NewItem_Inner orig, Terraria.DataStructures.IEntitySource source, int X, int Y, int Width, int Height, Item itemToClone, int Type, int Stack, bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup) {
			var clientconfig = ModContent.GetInstance<AutoTrashClientConfig>();
			bool suitableItemSpawn = source is EntitySource_Loot or EntitySource_TileBreak;
			if (suitableItemSpawn && clientconfig.PreventSpawn) {
				var fakeItem = new Item(Type); // ShouldItemBeTrashed needs an Item instance
				var autoTrashPlayer = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();
				if (autoTrashPlayer.AutoTrashEnabled && autoTrashPlayer.ShouldItemBeTrashed(fakeItem)) {
					return Main.maxItems;
				}
			}

			return orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryLayerIndex != -1) {
				layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash Slot",
					delegate {
						ModContent.GetInstance<AutoTrashGlobalItem>().DrawUpdateAutoTrash();
						return true;
					},
					InterfaceScaleType.UI)
				);

				layers.Insert(inventoryLayerIndex + 2, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash Cursor",
					delegate {
						if (Main.cursorOverride == 6 && (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl)) && Main.keyState.PressingShift()) {
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
			if (MouseTextIndex != -1) {
				layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
					"AutoTrash: Auto Trash List",
					delegate {
						if (AutoTrashListUI.visible) {

							AutoTrash.autoTrashUserInterface.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			if (AutoTrashListUI.visible) {
				AutoTrash.autoTrashUserInterface.Update(gameTime);
			}
		}
	}
}
