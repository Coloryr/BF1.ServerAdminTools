﻿namespace BF1.ServerAdminTools.Common.Data;

internal class ServerMapData
{
    public record MapName
    {
        public string ID;
        public string English;
        public string Chinese;
    }

    public static List<MapName> AllMapInfo { get; } = new()
    {
        new(){  }
    };
}
