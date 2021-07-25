using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using Microsoft.Xna.Framework.Input;

namespace AutoTrash
{
	class AutoTrashGlobalItem : GlobalItem
	{
		private static Item[] singleSlotArray;
		private static Texture2D clearTexture;

		public AutoTrashGlobalItem()
		{
			singleSlotArray = new Item[1];
		}

		public override bool Autoload(ref string name)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				clearTexture = mod.GetTexture("closeButton");
			}
			return base.Autoload(ref name);
		}

		public override bool OnPickup(Item item, Player player)
		{
			// TODO: IL edit maybe
			var autoTrashPlayer = player.GetModPlayer<AutoTrashPlayer>();
			//Main.NewText("ItemID: " + item.type);
			//foreach (var autoItme in autoTrashPlayer.AutoTrashItems)
			//{
			//	Main.NewText("Auto: " + autoItme.type);
			//}
			if (AutoTrashPlayer.IsModItem(item) && item.modItem.Name == "MysteryItem")
			{
				return true;
			}
			if (autoTrashPlayer.AutoTrashEnabled && autoTrashPlayer.ShouldItemBeTrashed(item))
			{
				autoTrashPlayer.LastAutoTrashItem = item;
				autoTrashPlayer.OnItemAutotrashed();
				//Main.item[j] = player.GetItem(player.whoAmI, Main.item[j], false, false);
				return false;
			}
			return true;
		}

		public override void CaughtFishStack(int type, ref int stack)
		{
			AutoTrashPlayer.caughtFish.Add(type);
		}

		internal void DrawUpdateAutoTrash()
		{
			if (Main.playerInventory)
			{
				Point mousePoint = new Point(Main.mouseX, Main.mouseY);

				// Calculate Position of ItemSlot
				Main.inventoryScale = 0.85f;

                var clientconfig = ModContent.GetInstance<AutoTrashClientConfig>();
				var serverconfig = ModContent.GetInstance<AutoTrashServerConfig>();

				int xPosition = 448;
				int yPosition = Main.instance.invBottom;
				if ((Main.LocalPlayer.chest != -1 && !Main.recBigList) || Main.npcShop > 0)
				{
					Main.inventoryScale = 0.755f;
					yPosition += 168;
					xPosition += 5;
				}
				xPosition -= (int)(56 * Main.inventoryScale);

				xPosition += (int)((clientconfig.SlotPositionX - 8) * (56 * Main.inventoryScale));
				yPosition += (int)((clientconfig.SlotPositionY - 5) * (56 * Main.inventoryScale));

				var autoTrashPlayer = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();
				// Toggle Button
				Texture2D inventoryTickTexture = Main.inventoryTickOnTexture;
				if (!autoTrashPlayer.AutoTrashEnabled)
				{
					inventoryTickTexture = Main.inventoryTickOffTexture;
				}
				Rectangle slotRectangle = new Rectangle(xPosition, yPosition, (int)((float)Main.inventoryBackTexture.Width * Main.inventoryScale), (int)((float)Main.inventoryBackTexture.Height * Main.inventoryScale));
				Rectangle enableButtonRectangle = new Rectangle(slotRectangle.Left + (int)(40 * Main.inventoryScale), slotRectangle.Top - 2, inventoryTickTexture.Width, inventoryTickTexture.Height);
				Rectangle listButtonRectangle = new Rectangle(slotRectangle.Left + (int)(40 * Main.inventoryScale) + 1, slotRectangle.Top + 13, inventoryTickTexture.Width, inventoryTickTexture.Height);
				Rectangle clearButtonRectangle = new Rectangle(slotRectangle.Left + (int)(40 * Main.inventoryScale), slotRectangle.Bottom - 9, inventoryTickTexture.Width, inventoryTickTexture.Height);

				bool enableButtonHover = false;
				bool listButtonHover = false;
				bool clearButtonHover = false;
				if (enableButtonRectangle.Contains(mousePoint))
				{
					Main.LocalPlayer.mouseInterface = true;
					enableButtonHover = true;
					if (Main.mouseLeft && Main.mouseLeftRelease)
					{
						autoTrashPlayer.AutoTrashEnabled = !autoTrashPlayer.AutoTrashEnabled;

						Main.mouseLeftRelease = false;
						Main.PlaySound(SoundID.MenuTick);
						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							//NetMessage.SendData(4, -1, -1, Main.LocalPlayer.name, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
						}
					}
				}
				if (listButtonRectangle.Contains(mousePoint))
				{
					Main.LocalPlayer.mouseInterface = true;
					listButtonHover = true;
					if (Main.mouseLeft && Main.mouseLeftRelease)
					{
						AutoTrash.instance.autoTrashListUI.UpdateNeeded();
						AutoTrashListUI.visible = !AutoTrashListUI.visible;
						Main.PlaySound(AutoTrashListUI.visible ? SoundID.MenuOpen : SoundID.MenuClose);
					}
				}
				if (clearButtonRectangle.Contains(mousePoint))
				{
					Main.LocalPlayer.mouseInterface = true;
					clearButtonHover = true;
					if (Main.mouseLeft && Main.mouseLeftRelease)
					{
						if (Main.keyState.IsKeyDown(Keys.LeftAlt) || Main.keyState.IsKeyDown(Keys.RightAlt))
						{
							autoTrashPlayer.AutoTrashItems.Clear();
							AutoTrash.instance.autoTrashListUI.UpdateNeeded();
							Main.mouseLeftRelease = false; // needed?
							Main.PlaySound(SoundID.MenuTick);
							autoTrashPlayer.LastAutoTrashItem.SetDefaults(0);
							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								//NetMessage.SendData(4, -1, -1, Main.LocalPlayer.name, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
							}
						}
						else
						{
							Main.NewText("You need to hold alt and click to clear the list");
						}
					}
				}


				singleSlotArray[0] = autoTrashPlayer.LastAutoTrashItem;
				if (slotRectangle.Contains(mousePoint) && !enableButtonHover && !clearButtonHover && !listButtonHover)
				{
					Main.LocalPlayer.mouseInterface = true;
					if (Main.mouseLeftRelease && Main.mouseLeft && autoTrashPlayer.AutoTrashEnabled)
					{
						int originalID = singleSlotArray[0].type;

						if (clientconfig.SellInstead) {
							float sellPercent = (serverconfig.SellValue >= 1 ? serverconfig.SellValue : 1) / 100f;
							var value = (int)Math.Floor(singleSlotArray[0].value * singleSlotArray[0].stack * sellPercent);
							if (!Main.mouseItem.IsAir || Main.LocalPlayer.BuyItem(value))
								Terraria.UI.ItemSlot.LeftClick(singleSlotArray, Terraria.UI.ItemSlot.Context.TrashItem);
							else
								Main.NewText("Not enough money to reclaim Auto-Sell item");
						}
						else
							Terraria.UI.ItemSlot.LeftClick(singleSlotArray, Terraria.UI.ItemSlot.Context.TrashItem);
						Recipe.FindRecipes();
						int newID = singleSlotArray[0].type;
						// Add 
						if (newID == 0)
						{
							// Remove from auto
							autoTrashPlayer.AutoTrashItems.RemoveAll(x => x.type == originalID);
						}
						else if (originalID != newID && newID != 0)
						{
							// add new to auto
							Item newItem = new Item();
							newItem.SetDefaults(newID);
							if (!autoTrashPlayer.AutoTrashItems.Any(x => x.type == newID))
								autoTrashPlayer.AutoTrashItems.Add(newItem);
						}
						AutoTrash.instance.autoTrashListUI.UpdateNeeded();
					}
					//ItemSlot.MouseHover(singleSlotArray, 6);
					Main.HoverItem = new Item();
					if (autoTrashPlayer.AutoTrashEnabled)
					{
						//if (Main.mouseItem.type > 0)
						//{
						//	Main.hoverItemName = "Place item to add to Auto-Trash list";
						//}
						//else
						//{
						Main.hoverItemName = singleSlotArray[0].type != ItemID.None 
							? (clientconfig.SellInstead ? "Click to remove from Auto-Sell list" : "Click to remove from Auto-Trash list") 
							: (clientconfig.SellInstead ? "Place item to add to Auto-Sell list" : "Place item to add to Auto-Trash list");
						//}
					}
					else
					{
						Main.hoverItemName = (clientconfig.SellInstead ? "Enable Auto-Sell to automatically sell items on pickup" : "Enable Auto-Trash to automatically trash items on pickup");
					}
				}
				singleSlotArray[0].newAndShiny = false;
				if (!autoTrashPlayer.AutoTrashEnabled)
				{
					Terraria.UI.ItemSlot.Draw(Main.spriteBatch, singleSlotArray, Terraria.UI.ItemSlot.Context.ChestItem, 0, new Vector2((float)xPosition, (float)yPosition), default(Color));
				}
				else if (clientconfig.SellInstead)
				{
					Main.spriteBatch.Draw(ModContent.GetTexture("AutoTrash/AutoSellInvSlot"), new Vector2(xPosition + 9, yPosition + 9), Color.White * 0.7f);
					Terraria.UI.ItemSlot.Draw(Main.spriteBatch, singleSlotArray, Terraria.UI.ItemSlot.Context.ShopItem, 0, new Vector2(xPosition, yPosition));
				} 
				else 
				{
					Terraria.UI.ItemSlot.Draw(Main.spriteBatch, singleSlotArray, Terraria.UI.ItemSlot.Context.TrashItem, 0, new Vector2((float)xPosition, (float)yPosition), default(Color));
				}
				bool itemChanged = autoTrashPlayer.LastAutoTrashItem != singleSlotArray[0];
				autoTrashPlayer.LastAutoTrashItem = singleSlotArray[0];
				if(itemChanged)
					autoTrashPlayer.OnItemAutotrashed();
				// want 0.7f??
				Main.spriteBatch.Draw(inventoryTickTexture, enableButtonRectangle.TopLeft(), Color.White * 0.7f);
				Main.spriteBatch.Draw(Main.instance.infoIconTexture[5], listButtonRectangle.TopLeft(), Color.White * 0.7f);
				Main.spriteBatch.Draw(clearTexture, clearButtonRectangle.TopLeft(), Color.White * 0.7f);
				if (enableButtonHover)
				{
					Main.HoverItem = new Item();
					Main.hoverItemName = autoTrashPlayer.AutoTrashEnabled 
						? (clientconfig.SellInstead ? "Auto-Sell Enabled: " : "Auto-Trash Enabled: ") + autoTrashPlayer.AutoTrashItems.Count + " items" 
						: (clientconfig.SellInstead ? "Auto-Sell Disabled: " : "Auto-Trash Disabled: ");
				}
				if (clearButtonHover)
				{
					Main.HoverItem = new Item();
					Main.hoverItemName = (clientconfig.SellInstead ? "Hold Alt and Click to Clear Auto-Sell list" : "Hold Alt and Click to Clear Auto-Trash list");
				}
				if (listButtonHover)
				{
					Main.HoverItem = new Item();
					Main.hoverItemName = (clientconfig.SellInstead ? "Click to View Auto-Sell list" : "Click to View Auto-Trash list");
				}

				Main.inventoryScale = 0.85f;

				//AutoTrashPlayer csp = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>(mod);
				//	if (r.Contains(value)/* && !flag2*/)
				//	{
				//		Main.LocalPlayer.mouseInterface = true;
				//		Main.armorHide = true;
				//		singleSlotArray[0] = accItem;
				//		ItemSlot.Handle(singleSlotArray, ItemSlot.Context.EquipAccessory, 0);
				//		accItem = singleSlotArray[0];
				//		//ItemSlot.Handle(ref accItem, ItemSlot.Context.EquipAccessory);
				//	}
				//	singleSlotArray[0] = accItem;
				//	ItemSlot.Draw(spriteBatch, singleSlotArray, 10, 0, new Vector2(r.X, r.Y));
				//	accItem = singleSlotArray[0];

				//	//ItemSlot.Draw(spriteBatch, ref accItem, 10, new Vector2(r.X, r.Y));

				//	csp.ExtraAccessories[i] = accItem;
			}
		}
	}
}
