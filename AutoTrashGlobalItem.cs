using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace AutoTrash
{
	class AutoTrashGlobalItem : GlobalItem
	{
		private static Item[] singleSlotArray;
		Texture2D clearTexture;

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
			/*
			Texture2D trashTexture = Main.trashTexture;
				Vector2 position5 = position + texture2D.Size() * inventoryScale / 2f - trashTexture.Size() * inventoryScale / 2f;
				spriteBatch.Draw(trashTexture, position5, null, new Color(100, 100, 100, 100), 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);
			*/
			if (Main.playerInventory)
			{
				Point value = new Point(Main.mouseX, Main.mouseY);

				// Calculate Position of ItemSlot
				Main.inventoryScale = 0.85f;
				int xPosition = 448;
				int yPosition = 258;
				if ((Main.LocalPlayer.chest != -1 && !Main.recBigList) || Main.npcShop > 0)
				{
					Main.inventoryScale = 0.755f;
					yPosition += 168;
					xPosition += 5;
				}
				xPosition -= (int)(56 * Main.inventoryScale);

				var autoTrashPlayer = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>(mod);
				// Toggle Button
				Texture2D texture2D = Main.inventoryTickOnTexture;
				if (!autoTrashPlayer.AutoTrashEnabled)
				{
					texture2D = Main.inventoryTickOffTexture;
				}
				Rectangle r = new Rectangle(xPosition, yPosition, (int)((float)Main.inventoryBackTexture.Width * Main.inventoryScale), (int)((float)Main.inventoryBackTexture.Height * Main.inventoryScale));
				Rectangle r2 = new Rectangle(r.Left + (int)(40 * Main.inventoryScale), r.Top - 2, texture2D.Width, texture2D.Height);
				Rectangle r3 = new Rectangle(r.Left + (int)(40 * Main.inventoryScale), r.Bottom - 9, texture2D.Width, texture2D.Height);

				bool smallButtonHover = false;
				bool clearButtonHover = false;
				if (r2.Contains(value))
				{
					Main.LocalPlayer.mouseInterface = true;
					smallButtonHover = true;
					if (Main.mouseLeft && Main.mouseLeftRelease)
					{
						autoTrashPlayer.AutoTrashEnabled = !autoTrashPlayer.AutoTrashEnabled;

						Main.mouseLeftRelease = false;
						Main.PlaySound(12, -1, -1, 1);
						if (Main.netMode == 1)
						{
							//NetMessage.SendData(4, -1, -1, Main.LocalPlayer.name, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
						}
					}
				}
				if (r3.Contains(value))
				{
					Main.LocalPlayer.mouseInterface = true;
					clearButtonHover = true;
					if (Main.mouseLeft && Main.mouseLeftRelease)
					{
						autoTrashPlayer.AutoTrashItems.Clear();
						Main.mouseLeftRelease = false;
						Main.PlaySound(12, -1, -1, 1);
						autoTrashPlayer.LastAutoTrashItem.SetDefaults(0);
						if (Main.netMode == 1)
						{
							//NetMessage.SendData(4, -1, -1, Main.LocalPlayer.name, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
						}
					}
				}


				singleSlotArray[0] = autoTrashPlayer.LastAutoTrashItem;
				if (r.Contains(value) && !smallButtonHover && !clearButtonHover)
				{
					Main.LocalPlayer.mouseInterface = true;
					if (Main.mouseLeftRelease && Main.mouseLeft && autoTrashPlayer.AutoTrashEnabled)
					{
						int originalID = singleSlotArray[0].type;
						ItemSlot.LeftClick(singleSlotArray, ItemSlot.Context.TrashItem);
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
					}
					//ItemSlot.MouseHover(singleSlotArray, 6);
					Main.toolTip = new Item();
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
					ItemSlot.Draw(Main.spriteBatch, singleSlotArray, ItemSlot.Context.ChestItem, 0, new Vector2((float)xPosition, (float)yPosition), default(Color));
				}
				else
				{
					ItemSlot.Draw(Main.spriteBatch, singleSlotArray, ItemSlot.Context.TrashItem, 0, new Vector2((float)xPosition, (float)yPosition), default(Color));
				}
				autoTrashPlayer.LastAutoTrashItem = singleSlotArray[0];
				Main.spriteBatch.Draw(texture2D, r2.TopLeft(), Color.White * 0.7f);
				//Texture2D clearTexture = mod.GetTexture("UI/closeButton");
				Main.spriteBatch.Draw(clearTexture, r3.TopLeft(), Color.White * 0.7f);
				if (smallButtonHover)
				{
					Main.toolTip = new Item();
					Main.hoverItemName = autoTrashPlayer.AutoTrashEnabled ? "Auto-Trash Enabled: " + autoTrashPlayer.AutoTrashItems.Count + " items" : "Auto-Trash Disabled";
				}
				if (clearButtonHover)
				{
					Main.toolTip = new Item();
					Main.hoverItemName = "Click to Clear Auto-Trash list";
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
