using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1.ServerAdminTools.Common.Data;

public class MapData
{
    public record MapName
    {
        public string ID;
        public string Chinese;
        public string English;
    }

    public static List<MapName> AllMapInfo { get; } = new()
    {
        new() { ID = "Levels/MP/MP_Amiens/MP_Amiens", Chinese = "亞眠", English = "Amiens" },
        new() { ID = "Levels/MP/MP_ItalianCoast/MP_ItalianCoast", Chinese = "帝國邊境", English = "ItalianCoast" },
        new() { ID = "Xpack1-3/Levels/MP_ShovelTown/MP_ShovelTown", Chinese = "攻佔托爾", English = "ShovelTown" },
        new() { ID = "Levels/MP/MP_MountainFort/MP_MountainFort", Chinese = "格拉巴山", English = "MountainFort" },
        new() { ID = "Xpack1/Levels/MP_Graveyard/MP_Graveyard", Chinese = "決裂", English = "Graveyard" },
        new() { ID = "Levels/MP/MP_FaoFortress/MP_FaoFortress", Chinese = "法歐堡", English = "Graveyard" },
        new() { ID = "Levels/MP/MP_Chateau/MP_Chateau", Chinese = "流血宴廳", English = "Chateau" },
        new() { ID = "Levels/MP/MP_Scar/MP_Scar", Chinese = "聖康坦的傷痕", English = "Scar" }
    };
}
