using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AutoTrash
{
	class AutoTrashClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// TODO: Custom UI instead.
		[DefaultValue(8)]
		[Range(0, 10)]
		public int SlotPositionX;

		[DefaultValue(5)]
		[Range(0, 10)]
		public int SlotPositionY;

		[DefaultValue(false)]
		public bool SellInstead { get; set; }

	}
}
