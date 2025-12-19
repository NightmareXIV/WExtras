global using ECommons.DalamudServices;
using Dalamud.Game;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.IPC;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using System;
using System.Reflection;

namespace WExtras
{
    public unsafe class WExtras : IDalamudPlugin
    {
        public string Name => "WeathermanExtras";
        WindowSystem ws;
        ConfigGui configGui;
        internal Config config;
        long nextCheck = 0;
        internal byte* TrueWeather;
        public WExtras(IDalamudPluginInterface pluginInterface)
        {
            ECommonsMain.Init(pluginInterface, this);
            config = Svc.PluginInterface.GetPluginConfig() as Config ?? new();
            ws = new();
            configGui = new(this);
            ws.AddWindow(configGui);
            Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
            Svc.Framework.Update += ApplySettings;
            TrueWeather = (byte*)(*(IntPtr*)Svc.SigScanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 48 83 C1 10 48 89 74 24") + 0x26);
        }

        private void ApplySettings(object framework)
        {
            if(Environment.TickCount64 > nextCheck)
            {
                nextCheck = Environment.TickCount64 + 1000;
                if((!config.AutoUnlockTimes || EnableAllTime()) && (!config.AutoUnlockWeathers || EnableAllWeather()))
                {
                    PluginLog.Debug("Unregistered ApplySettings");
                    Svc.Framework.Update -= ApplySettings;
                }
            }
        }

        public void Dispose()
        {
            Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
            Svc.Framework.Update -= ApplySettings;
            ECommonsMain.Dispose();
        }

        internal bool EnableAllTime()
        {
            try
            {
                var timeAllowedZones = ECommonsIPC.Weatherman.DataGetTimeAllowedZones();
                foreach(var z in Svc.Data.GetExcelSheet<TerritoryType>())
                {
                    timeAllowedZones.Add((ushort)z.RowId);
                }
                return true;
            }
            catch(Exception e)
            {
                PluginLog.Error($"{e.Message}\n{e.StackTrace}");
            }
            return false;
        }

        internal void SetWeather(byte weather)
        {
            try
            {
                ECommonsIPC.Weatherman.SetWeather(weather);
            }
            catch(Exception e)
            {
                PluginLog.Error($"{e.Message}\n{e.StackTrace}");
            }
        }

        internal bool EnableAllWeather()
        {
            try
            {
                var timeAllowedZones = ECommonsIPC.Weatherman.DataGetTimeAllowedZones();
                foreach(var z in Svc.Data.GetExcelSheet<TerritoryType>())
                {
                    timeAllowedZones.Add((ushort)z.RowId);
                }
                return true;
            }
            catch(Exception e)
            {
                //PluginLog.Error($"{e.Message}\n{e.StackTrace}");
            }
            return false;
        }

        internal Dictionary<byte, string> GetWeathers()
        {
            try
            {
                return ECommonsIPC.Weatherman.DataGetWeathers();
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}
