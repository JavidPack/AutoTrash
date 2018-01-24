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
			if (Main.netMode != 2)
			{
				clearTexture = mod.GetTexture("closeButton");
			}
			return base.Autoload(ref name);
		}

		public override bool OnPickup(Item item, Player player)
		{
			var autoTrashPlayer = player.GetModPlayer<AutoTrashPlayer>(mod);
			//Main.NewText("ItemID: " + item.type);
			//foreach (var autoItme in autoTrashPlayer.AutoTrashItems)
			//{
			//	Main.NewText("Auto: " + autoItme.type);
			//}
			if (AutoTrashPlayer.IsModItem(item) && item.modItem.mod.Name == "ModLoader")
			{
				return true;
			}
			if (autoTrashPlayer.AutoTrashEnabled && autoTrashPlayer.AutoTrashItems.Where(x => x.type == item.type).Any())
			{
				autoTrashPlayer.LastAutoTrashItem = item;
				//Main.item[j] = player.GetItem(player.whoAmI, Main.item[j], false, false);
				return false;
			}
			return true;
		}

		internal void DrawUpdateAutoTrash()
		{
			if (Main.playerInventory)
			{
				Point mousePoint = new Point(Main.mouseX, Main.mouseY);

				// Calculate Position of ItemSlot
				Main.inventoryScale = 0.85f;
				int xPosition = 448 + AutoTrash.config.XOffset;
				int yPosition = Main.instance.invBottom + AutoTrash.config.YOffset;
				if ((Main.LocalPlayer.chest != -1 && !Main.recBigList) || Main.npcShop > 0)
				{
					Main.inventoryScale = 0.755f;
					yPosition += 168;
					xPosition += 5;
				}
				xPosition -= (int)(56 * Main.inventoryScale);

				var autoTrashPlayer = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>(mod);
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
						if (Main.netMode == 1)
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
							if (Main.netMode == 1)
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
						Main.hoverItemName = singleSlotArray[0].type != 0 ? "Click to remove from Auto-Trash list" : "Place item to add to Auto-Trash list";
						//}
					}
					else
					{
						Main.hoverItemName = "Enable Auto-Trash to automatically trash items on pickup";
					}
				}
				singleSlotArray[0].newAndShiny = false;
				if (!autoTrashPlayer.AutoTrashEnabled)
				{
					Terraria.UI.ItemSlot.Draw(Main.spriteBatch, singleSlotArray, Terraria.UI.ItemSlot.Context.ChestItem, 0, new Vector2((float)xPosition, (float)yPosition), default(Color));
				}
				else
				{
					Terraria.UI.ItemSlot.Draw(Main.spriteBatch, singleSlotArray, Terraria.UI.ItemSlot.Context.TrashItem, 0, new Vector2((float)xPosition, (float)yPosition), default(Color));
				}
				autoTrashPlayer.LastAutoTrashItem = singleSlotArray[0];
				// want 0.7f??
				Main.spriteBatch.Draw(inventoryTickTexture, enableButtonRectangle.TopLeft(), Color.White * 0.7f);
				Main.spriteBatch.Draw(Main.instance.infoIconTexture[5], listButtonRectangle.TopLeft(), Color.White * 0.7f);
				Main.spriteBatch.Draw(clearTexture, clearButtonRectangle.TopLeft(), Color.White * 0.7f);
				if (enableButtonHover)
				{
					Main.HoverItem = new Item();
					Main.hoverItemName = autoTrashPlayer.AutoTrashEnabled ? "Auto-Trash Enabled: " + autoTrashPlayer.AutoTrashItems.Count + " items" : "Auto-Trash Disabled";
				}
				if (clearButtonHover)
				{
					Main.HoverItem = new Item();
					Main.hoverItemName = "Hold Alt and Click to Clear Auto-Trash list";
				}
				if (listButtonHover)
				{
					Main.HoverItem = new Item();
					Main.hoverItemName = "Click to View Auto-Trash list";
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
