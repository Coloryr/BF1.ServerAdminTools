namespace BF1.ServerAdminTools.Common.API.BF1Server;

public record UpdateServerReqBody
{
    public string jsonrpc { get; set; } = "2.0";
    public string method { get; set; } = "RSP.updateServer";
    public Params @params { get; set; }
    public string id { get; set; }
    public record Params
    {
        public DeviceIdMap deviceIdMap { get; set; }
        public string game { get; set; } = "tunguska";
        public string serverId { get; set; }
        public BannerSettings bannerSettings { get; set; }
        public MapRotation mapRotation { get; set; }
        public ServerSettings serverSettings { get; set; }
        public record DeviceIdMap
        {
            public string machash { get; set; }
        }
        public record BannerSettings
        {
            public string bannerUrl { get; set; }
            public bool clearBanner { get; set; }
        }
        public record MapRotation
        {
            public List<MapsItem> maps { get; set; }
            public string rotationType { get; set; }
            public string mod { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string id { get; set; }
            public record MapsItem
            {
                public string gameMode { get; set; }
                public string mapName { get; set; }
            }
        }
        public record ServerSettings
        {
            public string name { get; set; }
            public string description { get; set; }
            public string message { get; set; }
            public string password { get; set; }
            public string bannerUrl { get; set; }
            public string mapRotationId { get; set; }
            public string customGameSettings { get; set; }
        }
    }
}
