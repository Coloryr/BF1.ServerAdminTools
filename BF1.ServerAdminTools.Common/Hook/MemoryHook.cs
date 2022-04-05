﻿using BF1.ServerAdminTools.Common.Data;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Common.Utils;

namespace BF1.ServerAdminTools.Common.Hook;

internal static class MemoryHook
{
    private static IntPtr windowHandle;
    private static IntPtr processHandle;
    private static int processId;
    private static long processBaseAddress;

    public static bool Initialize(string ProcessName)
    {
        try
        {
            Globals.IsToolInit = false;
            LoggerHelper.Info($"目标程序名称 {ProcessName}");
            var pArray = Process.GetProcessesByName(ProcessName);
            if (pArray.Length > 0)
            {
                var process = pArray[0];
                windowHandle = process.MainWindowHandle;
                LoggerHelper.Info($"目标程序窗口句柄 {windowHandle}");
                processId = process.Id;
                LoggerHelper.Info($"目标程序进程ID {processId}");
                processHandle = WinAPI.OpenProcess(
                    ProcessAccessFlags.VirtualMemoryRead |
                    ProcessAccessFlags.VirtualMemoryWrite |
                    ProcessAccessFlags.VirtualMemoryOperation,
                    false, processId);
                LoggerHelper.Info($"目标程序进程句柄 {processHandle}");
                if (process.MainModule != null)
                {
                    processBaseAddress = process.MainModule.BaseAddress.ToInt64();
                    LoggerHelper.Info($"目标程序主模块基址 0x{processBaseAddress:x}");
                    Globals.IsToolInit = true;
                    return true;
                }
                else
                {
                    LoggerHelper.Error($"发生错误，目标程序主模块基址为空");
                    return false;
                }
            }
            else
            {
                LoggerHelper.Error($"发生错误，未发现目标进程");
                return false;
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.Error($"战地1内存模块初始化异常", ex);
            return false;
        }
    }

    public static void CloseHandle()
    {
        if (processHandle != IntPtr.Zero)
            WinAPI.CloseHandle(processHandle);
    }

    public static IntPtr GetWindowHandle()
    {
        return windowHandle;
    }

    public static IntPtr GetHandle()
    {
        return processHandle;
    }

    public static long GetBaseAddress()
    {
        return processBaseAddress;
    }

    public static bool IsTopMostWindow()
    {
        return windowHandle == WinAPI.GetForegroundWindow();
    }

    public static void SetForegroundWindow()
    {
        WinAPI.SetForegroundWindow(windowHandle);
    }

    public static int GetProcessId()
    {
        return processId;
    }

    private static long GetPtrAddress(long pointer, int[] offset)
    {
        if (offset != null)
        {
            byte[] buffer = new byte[8];
            WinAPI.ReadProcessMemory(processHandle, pointer, buffer, buffer.Length, out _);

            for (int i = 0; i < (offset.Length - 1); i++)
            {
                pointer = BitConverter.ToInt64(buffer, 0) + offset[i];
                WinAPI.ReadProcessMemory(processHandle, pointer, buffer, buffer.Length, out _);
            }

            pointer = BitConverter.ToInt64(buffer, 0) + offset[offset.Length - 1];
        }

        return pointer;
    }

    public static T Read<T>(long basePtr, int[] offsets) where T : struct
    {
        byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
        WinAPI.ReadProcessMemory(processHandle, GetPtrAddress(basePtr, offsets), buffer, buffer.Length, out _);
        return ByteArrayToStructure<T>(buffer);
    }

    public static T Read<T>(long address) where T : struct
    {
        byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
        WinAPI.ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        return ByteArrayToStructure<T>(buffer);
    }

    public static void Write<T>(long basePtr, int[] offsets, T value) where T : struct
    {
        byte[] buffer = StructureToByteArray(value);
        WinAPI.WriteProcessMemory(processHandle, GetPtrAddress(basePtr, offsets), buffer, buffer.Length, out _);
    }

    public static void Write<T>(long address, T value) where T : struct
    {
        byte[] buffer = StructureToByteArray(value);
        WinAPI.WriteProcessMemory(processHandle, address, buffer, buffer.Length, out _);
    }

    public static string ReadString(long address, int size)
    {
        byte[] buffer = new byte[size];
        WinAPI.ReadProcessMemory(processHandle, address, buffer, size, out _);

        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == 0)
            {
                byte[] _buffer = new byte[i];
                Buffer.BlockCopy(buffer, 0, _buffer, 0, i);
                return Encoding.ASCII.GetString(_buffer);
            }
        }

        return Encoding.ASCII.GetString(buffer);
    }

    public static string ReadString(long basePtr, int[] offsets, int size)
    {
        byte[] buffer = new byte[size];
        WinAPI.ReadProcessMemory(processHandle, GetPtrAddress(basePtr, offsets), buffer, size, out _);

        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == 0)
            {
                byte[] _buffer = new byte[i];
                Buffer.BlockCopy(buffer, 0, _buffer, 0, i);
                return Encoding.ASCII.GetString(_buffer);
            }
        }

        return Encoding.ASCII.GetString(buffer);
    }

    public static void WriteString(long basePtr, int[] offsets, string str)
    {
        byte[] buffer = new ASCIIEncoding().GetBytes(str);
        WinAPI.WriteProcessMemory(processHandle, GetPtrAddress(basePtr, offsets), buffer, buffer.Length, out _);
    }

    public static void WriteStringUTF8(long basePtr, int[] offsets, string str)
    {
        byte[] buffer = new UTF8Encoding().GetBytes(str);
        WinAPI.WriteProcessMemory(processHandle, GetPtrAddress(basePtr, offsets), buffer, buffer.Length, out _);
    }

    //////////////////////////////////////////////////////////////////

    public static bool IsValid(long Address)
    {
        return Address >= 0x10000 && Address < 0x000F000000000000;
    }

    private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            var obj = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            if (obj != null)
                return (T)obj;
            else
                return default(T);
        }
        finally
        {
            handle.Free();
        }
    }

    private static byte[] StructureToByteArray(object obj)
    {
        int length = Marshal.SizeOf(obj);
        byte[] array = new byte[length];
        IntPtr pointer = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(obj, pointer, true);
        Marshal.Copy(pointer, array, 0, length);
        Marshal.FreeHGlobal(pointer);
        return array;
    }

    private static TempData.ClientPlayer _tdCP;
    private static TempData.ClientSoldierEntity _tdCSE;


    private const int MaxPlayer = 74;

    static MemoryHook()
    {
        _tdCP.WeaponSlot = new string[8] { "", "", "", "", "", "", "", "" };
    }

    public static void Tick()
    {
        //////////////////////////////// 数据初始化 ////////////////////////////////

        Globals.statisticData_Team1.MaxPlayerCount = 0;
        Globals.statisticData_Team1.PlayerCount = 0;
        Globals.statisticData_Team1.Rank150PlayerCount = 0;
        Globals.statisticData_Team1.AllKillCount = 0;
        Globals.statisticData_Team1.AllDeadCount = 0;

        Globals.statisticData_Team2.MaxPlayerCount = 0;
        Globals.statisticData_Team2.PlayerCount = 0;
        Globals.statisticData_Team2.Rank150PlayerCount = 0;
        Globals.statisticData_Team2.AllKillCount = 0;
        Globals.statisticData_Team2.AllDeadCount = 0;

        Globals.PlayerList_All.Clear();
        Globals.PlayerList_Team0.Clear();
        Globals.PlayerList_Team1.Clear();
        Globals.PlayerList_Team2.Clear();

        Globals.Server_SpectatorList.Clear();

        Array.Clear(_tdCP.WeaponSlot, 0, _tdCP.WeaponSlot.Length);

        Globals.localPlayer.BaseAddress = Player.GetLocalPlayer();
        Globals.localPlayer.TeamID = Read<int>(Globals.localPlayer.BaseAddress + 0x1C34);

        Globals.localPlayer.Spectator = Read<byte>(Globals.localPlayer.BaseAddress + 0x1C31);
        Globals.localPlayer.PersonaId = Read<long>(Globals.localPlayer.BaseAddress + 0x38);
        Globals.localPlayer.PlayerName = ReadString(Globals.localPlayer.BaseAddress + 0x2156, 64);

        //////////////////////////////// 玩家数据 ////////////////////////////////

        for (int i = 0; i < MaxPlayer; i++)
        {
            _tdCP.BaseAddress = Player.GetPlayerById(i);
            if (!IsValid(_tdCP.BaseAddress))
                continue;

            _tdCP.Mark = Read<byte>(_tdCP.BaseAddress + 0x1D7C);
            _tdCP.TeamID = Read<int>(_tdCP.BaseAddress + 0x1C34);
            _tdCP.Spectator = Read<byte>(_tdCP.BaseAddress + 0x1C31);
            _tdCP.PersonaId = Read<long>(_tdCP.BaseAddress + 0x38);
            _tdCP.PartyId = Read<int>(_tdCP.BaseAddress + 0x1E50);
            _tdCP.Name = ReadString(_tdCP.BaseAddress + 0x2156, 64);
            if (string.IsNullOrEmpty(_tdCP.Name))
                continue;

            _tdCSE.pClientVehicleEntity = Read<long>(_tdCP.BaseAddress + 0x1D38);
            if (IsValid(_tdCSE.pClientVehicleEntity))
            {
                _tdCSE.pVehicleEntityData = Read<long>(_tdCSE.pClientVehicleEntity + 0x30);
                _tdCP.WeaponSlot[0] = ReadString(Read<long>(_tdCSE.pVehicleEntityData + 0x2F8), 64);

                for (int j = 1; j < 8; j++)
                {
                    _tdCP.WeaponSlot[j] = "";
                }
            }
            else
            {
                _tdCSE.pClientSoldierEntity = Read<long>(_tdCP.BaseAddress + 0x1D48);
                _tdCSE.pClientSoldierWeaponComponent = Read<long>(_tdCSE.pClientSoldierEntity + 0x698);
                _tdCSE.m_handler = Read<long>(_tdCSE.pClientSoldierWeaponComponent + 0x8A8);

                for (int j = 0; j < 8; j++)
                {
                    var offset0 = Read<long>(_tdCSE.m_handler + j * 0x8);

                    offset0 = Read<long>(offset0 + 0x4A30);
                    offset0 = Read<long>(offset0 + 0x20);
                    offset0 = Read<long>(offset0 + 0x38);
                    offset0 = Read<long>(offset0 + 0x20);

                    _tdCP.WeaponSlot[j] = ReadString(offset0, 64);
                }
            }

            int index = Globals.PlayerList_All.FindIndex(val => val.Name == _tdCP.Name);
            if (index == -1)
            {
                Globals.PlayerList_All.Add(new PlayerData()
                {
                    Mark = _tdCP.Mark,
                    TeamID = _tdCP.TeamID,
                    Spectator = _tdCP.Spectator,
                    Clan = PlayerUtil.GetPlayerTargetName(_tdCP.Name, true),
                    Name = PlayerUtil.GetPlayerTargetName(_tdCP.Name, false),
                    PersonaId = _tdCP.PersonaId,
                    SquadId = PlayerUtil.GetSquadChsName(_tdCP.PartyId),

                    Rank = 0,
                    Kill = 0,
                    Dead = 0,
                    Score = 0,

                    KD = 0,
                    KPM = 0,

                    WeaponS0 = _tdCP.WeaponSlot[0],
                    WeaponS1 = _tdCP.WeaponSlot[1],
                    WeaponS2 = _tdCP.WeaponSlot[2],
                    WeaponS3 = _tdCP.WeaponSlot[3],
                    WeaponS4 = _tdCP.WeaponSlot[4],
                    WeaponS5 = _tdCP.WeaponSlot[5],
                    WeaponS6 = _tdCP.WeaponSlot[6],
                    WeaponS7 = _tdCP.WeaponSlot[7],

                    WeaponS0CH = PlayerUtil.GetWeaponChsName(_tdCP.WeaponSlot[0]),
                    WeaponS1CH = PlayerUtil.GetWeaponChsName(_tdCP.WeaponSlot[1]),
                    WeaponS2CH = PlayerUtil.GetWeaponChsName(_tdCP.WeaponSlot[2]),
                    WeaponS3CH = PlayerUtil.GetWeaponChsName(_tdCP.WeaponSlot[3]),
                    WeaponS4CH = PlayerUtil.GetWeaponChsName(_tdCP.WeaponSlot[4]),
                    WeaponS5CH = PlayerUtil.GetWeaponChsName(_tdCP.WeaponSlot[5]),
                    WeaponS6CH = PlayerUtil.GetWeaponChsName(_tdCP.WeaponSlot[6]),
                    WeaponS7CH = PlayerUtil.GetWeaponChsName(_tdCP.WeaponSlot[7]),
                });
            }
        }

        //////////////////////////////// 得分板数据 ////////////////////////////////

        var pClientScoreBA = Read<long>(GetBaseAddress() + 0x39EB8D8);
        pClientScoreBA = Read<long>(pClientScoreBA + 0x68);

        for (int i = 0; i < MaxPlayer; i++)
        {
            pClientScoreBA = Read<long>(pClientScoreBA);
            var pClientScoreOffset = Read<long>(pClientScoreBA + 0x10);
            if (!IsValid(pClientScoreBA))
                continue;

            var Mark = Read<byte>(pClientScoreOffset + 0x300);
            var Rank = Read<int>(pClientScoreOffset + 0x304);
            if (Rank == 0)
                continue;
            var Kill = Read<int>(pClientScoreOffset + 0x308);
            var Dead = Read<int>(pClientScoreOffset + 0x30C);
            var Score = Read<int>(pClientScoreOffset + 0x314);

            int index = Globals.PlayerList_All.FindIndex(val => val.Mark == Mark);
            if (index != -1)
            {
                Globals.PlayerList_All[index].Rank = Rank;
                Globals.PlayerList_All[index].Kill = Kill;
                Globals.PlayerList_All[index].Dead = Dead;
                Globals.PlayerList_All[index].Score = Score;
                Globals.PlayerList_All[index].KD = PlayerUtil.GetPlayerKD(Kill, Dead);
                Globals.PlayerList_All[index].KPM = PlayerUtil.GetPlayerKPM(Kill, PlayerUtil.SecondsToMM(Globals.serverInfo.ServerTime));
            }
        }

        //////////////////////////////// 队伍数据整理 ////////////////////////////////

        foreach (var item in Globals.PlayerList_All)
        {
            item.Admin = PlayerUtil.CheckAdminVIP(item.PersonaId, Globals.Server_AdminList);
            item.VIP = PlayerUtil.CheckAdminVIP(item.PersonaId, Globals.Server_VIPList);

            if (item.TeamID == 0)
            {
                Globals.PlayerList_Team0.Add(item);
            }
            if (item.TeamID == 1)
            {
                Globals.PlayerList_Team1.Add(item);
            }
            else if (item.TeamID == 2)
            {
                Globals.PlayerList_Team2.Add(item);
            }
        }

        // 观战玩家信息
        foreach (var item in Globals.PlayerList_Team0)
        {
            Globals.Server_SpectatorList.Add(new SpectatorInfo()
            {
                Name = item.Name,
                PersonaId = item.PersonaId.ToString(),
            });
        }

        // 队伍1数据统计
        foreach (var item in Globals.PlayerList_Team1)
        {
            // 统计当前服务器玩家数量
            if (item.Rank != 0)
            {
                Globals.statisticData_Team1.MaxPlayerCount++;
            }

            // 统计当前服务器存活玩家数量
            if (item.WeaponS0 != "")
            {
                Globals.statisticData_Team1.PlayerCount++;
            }

            // 统计当前服务器150级玩家数量
            if (item.Rank == 150)
            {
                Globals.statisticData_Team1.Rank150PlayerCount++;
            }

            // 总击杀总死亡数统计
            Globals.statisticData_Team1.AllKillCount += item.Kill;
            Globals.statisticData_Team1.AllDeadCount += item.Dead;
        }

        // 队伍2数据统计
        foreach (var item in Globals.PlayerList_Team2)
        {
            // 统计当前服务器玩家数量
            if (item.Rank != 0)
            {
                Globals.statisticData_Team2.MaxPlayerCount++;
            }

            // 统计当前服务器存活玩家数量
            if (item.WeaponS0 != "" ||
                item.WeaponS1 != "" ||
                item.WeaponS2 != "" ||
                item.WeaponS3 != "" ||
                item.WeaponS4 != "" ||
                item.WeaponS5 != "" ||
                item.WeaponS6 != "" ||
                item.WeaponS7 != "")
            {
                Globals.statisticData_Team2.PlayerCount++;
            }

            // 统计当前服务器150级玩家数量
            if (item.Rank == 150)
            {
                Globals.statisticData_Team2.Rank150PlayerCount++;
            }

            Globals.statisticData_Team2.AllKillCount += item.Kill;
            Globals.statisticData_Team2.AllDeadCount += item.Dead;
        }

        ////////////////////////////////////////////////////////////////////////////////

        // 服务器名称
        Globals.serverInfo.ServerName = ReadString(GetBaseAddress() + Offsets.ServerName_Offset, Offsets.ServerName, 64);
        Globals.serverInfo.ServerName = Globals.serverInfo.ServerName == "" ? "未知" : Globals.serverInfo.ServerName;

        // 如果玩家没有进入服务器，要进行一些数据清理
        if (Globals.PlayerList_Team1.Count == 0 && Globals.PlayerList_Team2.Count == 0 && Globals.serverInfo.ServerName == "未知")
        {
            // 清理服务器ID（GameID）
            Globals.serverInfo.ServerID = 0;
            Globals.Config.GameId = string.Empty;

            Globals.Server_AdminList.Clear();
            Globals.Server_Admin2List.Clear();
            Globals.Server_VIPList.Clear();
        }
        else
        {
            // 服务器数字ID
            Globals.serverInfo.ServerID = Read<long>(GetBaseAddress() + Offsets.ServerID_Offset, Offsets.ServerID);
            Globals.Config.GameId = Globals.serverInfo.ServerID.ToString();
        }

        // 服务器时间
        Globals.serverInfo.ServerTime = Read<float>(GetBaseAddress() + Offsets.ServerTime_Offset, Offsets.ServerTime);

        Globals.serverInfo.Offset0 = Read<long>(GetBaseAddress() + Offsets.ServerScore_Offset, Offsets.ServerScoreTeam);

        // 队伍1、队伍2分数
        Globals.serverInfo.Team1Score = Read<int>(Globals.serverInfo.Offset0 + 0x2B0);
        Globals.serverInfo.Team2Score = Read<int>(Globals.serverInfo.Offset0 + 0x2B0 + 0x08);

        // 队伍1、队伍2从击杀获取得分
        Globals.serverInfo.Team1FromeKill = Read<int>(Globals.serverInfo.Offset0 + 0x2B0 + 0x60);
        Globals.serverInfo.Team2FromeKill = Read<int>(Globals.serverInfo.Offset0 + 0x2B0 + 0x68);

        // 队伍1、队伍2从旗帜获取得分
        Globals.serverInfo.Team1FromeFlag = Read<int>(Globals.serverInfo.Offset0 + 0x2B0 + 0x100);
        Globals.serverInfo.Team2FromeFlag = Read<int>(Globals.serverInfo.Offset0 + 0x2B0 + 0x108);

        if (Globals.serverInfo.Team1FromeFlag < 0 || Globals.serverInfo.Team1FromeFlag > 2000)
        {
            Globals.serverInfo.Team1FromeFlag = 0;
        }

        if (Globals.serverInfo.Team1FromeKill < 0 || Globals.serverInfo.Team1FromeKill > 2000)
        {
            Globals.serverInfo.Team1FromeKill = 0;
        }

        if (Globals.serverInfo.Team2FromeFlag < 0 || Globals.serverInfo.Team2FromeFlag > 2000)
        {
            Globals.serverInfo.Team2FromeFlag = 0;
        }

        if (Globals.serverInfo.Team2FromeKill < 0 || Globals.serverInfo.Team2FromeKill > 2000)
        {
            Globals.serverInfo.Team2FromeKill = 0;
        }

        // 暴露给外部使用
        lock (Globals.PlayerDatas_Team1)
        {
            Globals.PlayerDatas_Team1.Clear();
            foreach (var item in Globals.PlayerList_Team1)
            {
                if (Globals.PlayerDatas_Team1.ContainsKey(item.PersonaId))
                    Globals.PlayerDatas_Team1[item.PersonaId] = item;
                else
                    Globals.PlayerDatas_Team1.Add(item.PersonaId, item);
            }
        }
        lock (Globals.PlayerDatas_Team2)
        {
            Globals.PlayerDatas_Team2.Clear();
            foreach (var item in Globals.PlayerList_Team2)
            {
                if (Globals.PlayerDatas_Team2.ContainsKey(item.PersonaId))
                    Globals.PlayerDatas_Team2[item.PersonaId] = item;
                else
                    Globals.PlayerDatas_Team2.Add(item.PersonaId, item);
            }
        }
    }
}