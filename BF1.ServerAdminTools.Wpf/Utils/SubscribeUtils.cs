using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Wpf.Data;
using RestSharp;

namespace BF1.ServerAdminTools.Wpf.Utils;

internal static class SubscribeUtils
{
    private static RestClient client;
    private static bool IsEdit = false;
    static SubscribeUtils()
    {
        if (client == null)
        {
            var options = new RestClientOptions()
            {
                Timeout = 10000
            };

            client = new RestClient(options);
        }
    }

    /// <summary>
    /// 添加订阅
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<RespSubscribe> Add(string url)
    {
        RespSubscribe res = new();

        foreach (var item in DataSave.Subscribes.UrlList)
        {
            if (item == url)
            {
                return res;
            }
        }

        try
        {
            var req = new RestRequest(url);
            var res1 = await client.ExecuteGetAsync(req);
            var obj = JsonUtils.JsonDese<HttpSubscribe>(res1.Content);

            if (obj == null)
            {
                return res;
            }

            if (obj.Players == null)
            {
                res.http = obj;
                return res;
            }

            var obj1 = new SubscribeObj()
            {
                Url = url,
                Players = obj.Players,
                LastTime = obj.Time,
                UpdateTime = DateTime.Now.ToString()
            };

            DataSave.Subscribes.UrlList.Add(url);
            WpfConfigUtils.SaveSubscribe();
            DataSave.SubscribeCache.Cache.Add(obj1);
            WpfConfigUtils.SaveSubscribeCache();
            res.obj = obj1;
            res.OK = true;
        }
        catch
        { }

        return res;
    }

    /// <summary>
    /// 全部订阅更新
    /// </summary>
    /// <returns></returns>
    public static async Task UpdateAll()
    {
        IsEdit = true;
        foreach (var url in DataSave.Subscribes.UrlList)
        {
            try
            {
                var req = new RestRequest(url);
                var res1 = await client.ExecuteGetAsync(req);
                var obj = JsonUtils.JsonDese<HttpSubscribe>(res1.Content);

                if (obj == null || obj.Players == null)
                {
                    continue;
                }
                foreach (var item in DataSave.SubscribeCache.Cache)
                {
                    if (item.Url == url)
                    {
                        item.Players = obj.Players;
                        item.LastTime = obj.Time;
                    }
                }

            }
            catch
            { }
        }
        IsEdit = false;
        WpfConfigUtils.SaveSubscribeCache();
    }

    /// <summary>
    /// 删除订阅
    /// </summary>
    /// <param name="url"></param>
    public static void Delete(string url)
    {
        DataSave.Subscribes.UrlList.Remove(url);
        WpfConfigUtils.SaveSubscribe();
        SubscribeObj obj = null;
        foreach (var item in DataSave.SubscribeCache.Cache)
        {
            if (item.Url == url)
            {
                obj = item;
            }
        }
        if (obj != null)
        {
            DataSave.SubscribeCache.Cache.Remove(obj);
            WpfConfigUtils.SaveSubscribeCache();
        }
    }

    /// <summary>
    /// 检测是否在黑名单中
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool Check(long pid, string name)
    {
        if (IsEdit)
            return false;

        foreach (var item in DataSave.SubscribeCache.Cache)
        {
            if (IsEdit)
                return false;

            foreach (var item1 in item.Players)
            {
                //全部匹配
                if (item1.ID != 0 && !string.IsNullOrWhiteSpace(item1.Name))
                {
                    if (item1.ID == pid && item1.Name == name)
                        return true;
                }
                else if (item1.ID != 0)
                {
                    if (item1.ID == pid)
                        return true;
                }
                else if (!string.IsNullOrWhiteSpace(item1.Name))
                {
                    if (item1.Name == name)
                        return true;
                }
            }
        }

        return false;
    }
}