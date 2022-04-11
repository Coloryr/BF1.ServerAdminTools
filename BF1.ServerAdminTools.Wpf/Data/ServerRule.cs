﻿namespace BF1.ServerAdminTools.Wpf.Data;

public record ServerRule
{
    public string Name { get; set; }
    public int MaxKill { get; set; }

    public int KDFlag { get; set; }
    public float MaxKD { get; set; }

    public int KPMFlag { get; set; }
    public float MaxKPM { get; set; }

    public int MaxRank { get; set; }
    public int MinRank { get; set; }

    public float LifeMaxKD { get; set; }
    public float LifeMaxKPM { get; set; }
    public int LifeMaxWeaponStar { get; set; }
    public int LifeMaxVehicleStar { get; set; }

    public int ScoreSwitchMap { get; set; }
    public int ScoreNotSwitchMap { get; set; }
    public bool RandomSwitchMap { get; set; }

    public int ScoreOtherRule { get; set; }
    public string OtherRule { get; set; }

    /// <summary>
    /// 保存限制武器名称列表
    /// </summary>
    public List<string> Custom_WeaponList { get; set; } = new();
    /// <summary>
    /// 自定义黑名单玩家列表
    /// </summary>
    public List<string> Custom_BlackList { get; set; } = new();
    /// <summary>
    /// 自定义白名单玩家列表
    /// </summary>
    public List<string> Custom_WhiteList { get; set; } = new();
}


