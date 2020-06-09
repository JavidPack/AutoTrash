using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AutoTrash
{
	class AutoTrashClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(8)]
		[Label("AutoTrash Slot Icon Position X")]
		[Tooltip("Customize the position of the AutoTrash slot measured left to right from the 1st hotbar")]
		[Range(0, 10)]
		public int SlotPositionX;

		[DefaultValue(5)]
		[Label("AutoTrash Slot Icon Position Y")]
		[Tooltip("Customize the position of the AutoTrash slot measured top to bottom from the top hotbar")]
		[Range(0, 10)]
		public int SlotPositionY;
		
		[DefaultValue(false)]
		[Label("Sell items instead")]
		[Tooltip("Will sell items on pickup instead of trashing them")]
		public bool SellInstead { get; set; }
		
		[DefaultValue(15)]
		[Label("AutoSell % sell value")]
		[Tooltip("Customize the sell value of a item, default merchant sell value is 20%, mod's default sell value is ~15%")]
		[Range(1, 100)]
		public int SellValue;
	}
}
