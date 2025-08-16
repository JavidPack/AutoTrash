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

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) {
			message = Mod.GetLocalization("Configs.AutoTrashServerConfig.RejectReason").ToNetworkText();
			return false;
		}
	}
}
