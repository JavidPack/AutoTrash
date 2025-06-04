using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace AutoTrash
{
	class AutoTrashServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(15)]
		[Range(1, 100)]
		public int SellValue;

		// Not yet tested but should work in theory to prevent clients from changing config to buy items for cheap then changing it again to sell at high value
		// This should help with multiplayer compatibility
		// For testing before enable
		/*
        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
        {
			message = Language.GetText("Mods.AutoTrash.Configs.AutoTrashServerConfig.RejectReason").ToNetworkText();
			return false;
        }
		*/
    }
}
