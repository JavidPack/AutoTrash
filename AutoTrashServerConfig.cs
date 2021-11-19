using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AutoTrash
{
	class AutoTrashServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(15)]
		[Label("AutoSell % sell value")]
		[Tooltip("Customize the sell value of a item, default merchant sell value is 20%, mod's default sell value is ~15%")]
		[Range(1, 100)]
		public int SellValue;
	}
}
