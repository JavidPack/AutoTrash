﻿using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace AutoTrash
{
	class AutoTrashServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		#pragma warning disable 0649
		[DefaultValue(15)]
		[Label("AutoSell % sell value")]
		[Tooltip("Customize the sell value of a item, default merchant sell value is 20%, mod's default sell value is ~15%")]
		[Range(1, 100)]
		public int SellValue;
		#pragma warning restore 0649

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		{
			string deny = "You do not have proper permission.";
			string accept = "Your changes have been accepted.";

			if (AutoTrash.instance.herosmod != null)
			{
				if (AutoTrash.instance.herosmod.Call("HasPermission", whoAmI, AutoTrash.heropermission) is bool result && result)
				{
					message = accept;
					return true;
				}
			}
			else
			{
				message = accept;
				return true;
			}
			message = deny;
			return false;
		}

    }

}
