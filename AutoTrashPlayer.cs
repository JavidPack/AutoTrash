using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace AutoTrash
{
	internal class AutoTrashPlayer : ModPlayer
	{
		public bool AutoTrashEnabled = false;
		public List<Item> AutoTrashItems;
		public Item LastAutoTrashItem;

		public bool NoValue;
		internal bool NoValueBelongs(Item item) => item.value == 0 && !ItemID.Sets.NebulaPickup[item.type] && !heartsAndMana.Contains(item.type);
		internal static int[] heartsAndMana = new int[] { 58, 1734, 1867, 184 , 1735 , 1868 };

		public override void Initialize()
		{
			AutoTrashItems = new List<Item>();
			LastAutoTrashItem = new Item();
			LastAutoTrashItem.SetDefaults(0, true);
			AutoTrashEnabled = false;
			NoValue = false;
		}

		public override TagCompound Save()
		{
			return new TagCompound
			{
				["AutoTrashItems"] = AutoTrashItems,
				["AutoTrashEnabled"] = AutoTrashEnabled,
				[nameof(NoValue)] = NoValue,
			};
		}

		public override void Load(TagCompound tag)
		{
			AutoTrashItems = tag.Get<List<Item>>("AutoTrashItems");
			AutoTrashEnabled = tag.GetBool("AutoTrashEnabled");
			NoValue = tag.GetBool(nameof(NoValue));

			AutoTrash.instance.autoTrashListUI?.UpdateNeeded();
		}

		internal static bool IsModItem(Item item)
		{
			return item.type >= ItemID.Count;
		}

		internal bool ShouldItemBeTrashed(Item item)
		{
			return AutoTrashItems.Where(x => x.type == item.type).Any() || (NoValue && NoValueBelongs(item));
		}

		public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
		{
			if (context == Terraria.UI.ItemSlot.Context.InventoryItem || context == Terraria.UI.ItemSlot.Context.InventoryCoin || context == Terraria.UI.ItemSlot.Context.InventoryAmmo)
			{
				if (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl))
				{
					if (AutoTrashEnabled && (!AutoTrashItems.Any(x => x.type == inventory[slot].type) || ModContent.GetInstance<AutoTrashServerConfig>().SellInstead))
					{
						Main.PlaySound(SoundID.Grab, -1, -1, 1, 1f, 0f);

						if (!AutoTrashItems.Any(x => x.type == inventory[slot].type)) {
							Item newItem = new Item();
							newItem.SetDefaults(inventory[slot].type);
							AutoTrashItems.Add(newItem);
						}

						LastAutoTrashItem = inventory[slot].Clone();
						OnItemAutotrashed();

						inventory[slot].SetDefaults();

						AutoTrash.instance.autoTrashListUI.UpdateNeeded();
						return true;
					}
				}
			}
			return false; // do default behavior.
		}

		public static HashSet<int> caughtFish = new HashSet<int>();
		public override void PreUpdate()
		{
			// Fishing uses player.GetItem bypassing AutoTrash.
			if (Main.myPlayer == player.whoAmI)
			{
				if(caughtFish.Count > 0) // type vs Item filter?
				{
					var toDelete = AutoTrashItems.Select(x => x.type).Intersect(caughtFish);
					for (int i = 0; i < 59; i++)
					{
						Item item = player.inventory[i];
						if (!item.IsAir && item.newAndShiny && toDelete.Contains(item.type))
						{
							// TODO: Analyze performance impact? do every 60 frames only?
							LastAutoTrashItem = item.Clone();
							OnItemAutotrashed();
							item.TurnToAir();
							// break; Multi-lure catches multiple items in 1 tick
						}
					}
					caughtFish.Clear();
				}
			}
		}

		public void OnItemAutotrashed() {
			var clientconfig = ModContent.GetInstance<AutoTrashClientConfig>();
			var serverconfig = ModContent.GetInstance<AutoTrashServerConfig>();

			if (serverconfig.SellInstead && LastAutoTrashItem.value > 0 && !(LastAutoTrashItem.type >= ItemID.CopperCoin && LastAutoTrashItem.type <= ItemID.PlatinumCoin)) {
				float sellPercent = (serverconfig.SellValue >= 1 ? serverconfig.SellValue : 1) / 100f;
				var value = Math.Floor((double)(LastAutoTrashItem.value * LastAutoTrashItem.stack * sellPercent));

				var plat = Math.Floor(value / Item.platinum);
				var gold = Math.Floor((value - (plat * Item.platinum)) / Item.gold);
				var silver = Math.Floor((value - (plat * Item.platinum) - (gold * Item.gold)) / Item.silver);
				var copper = Math.Floor(value - (plat * Item.platinum) - (gold * Item.gold) - (silver * Item.silver));

				if (plat > 0) player.QuickSpawnItem(ItemID.PlatinumCoin, (int)plat);
				if (gold > 0) player.QuickSpawnItem(ItemID.GoldCoin, (int)gold);
				if (silver > 0) player.QuickSpawnItem(ItemID.SilverCoin, (int)silver);
				if (copper > 0) player.QuickSpawnItem(ItemID.CopperCoin, (int)copper);
			}
		}
	}
}
