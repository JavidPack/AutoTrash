using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AutoTrash
{
	class AutoTrashServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(15)]
		[Range(1, 100)]
		public int SellValue;
	}
}
