
using Terraria;


namespace AutoTrash
{
    static public class AutoTrashInterface
    {
        static public bool isItemAutoTrashedByPlayer(Player player, int item_type)
        {
            // Returns False if AutoTrash is disabled
            // Returns False if item is not 'AutoTrashed'
            // Returns True if item is 'AutoTrashed'
            var mod_player = player.GetModPlayer<AutoTrashPlayer>();

            if (!mod_player.AutoTrashEnabled)
                return false;

            foreach (var item in mod_player.AutoTrashItems)
            {
                if (item_type == item.type)
                    return true;
            }

            return false;
        }
    }
}
