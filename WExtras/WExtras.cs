﻿using Dalamud.Game;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Reflection;

namespace WExtras
{
    public class WExtras : IDalamudPlugin
    {
        public string Name => "WeathermanExtras";
        WindowSystem ws;
        ConfigGui configGui;
        internal Config config;
        long nextCheck = 0;
        public WExtras(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            config = Svc.PluginInterface.GetPluginConfig() as Config ?? new();
            ws = new();
            configGui = new(this);
            ws.AddWindow(configGui);
            Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
            Svc.Framework.Update += ApplySettings;
        }

        private void ApplySettings(Framework framework)
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
        }

        internal bool EnableAllTime()
        {
            try
            {
                var plugin = GetWeathermanPlugin();
                var timeAllowedZones = (HashSet<ushort>)plugin.GetType().GetField("timeAllowedZones", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(plugin);
                foreach (var z in Svc.Data.GetExcelSheet<TerritoryType>())
                {
                    timeAllowedZones.Add((ushort)z.RowId);
                }
                return true;
            }
            catch (Exception e)
            {
                //PluginLog.Error($"{e.Message}\n{e.StackTrace}");
            }
            return false;
        }

        internal void SetWeather(byte weather)
        {
            try
            {
                var plugin = GetWeathermanPlugin();
                plugin.GetType().GetField("SelectedWeather", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(plugin, weather);
            }
            catch (Exception e)
            {
                PluginLog.Error($"{e.Message}\n{e.StackTrace}");
            }
        }

        internal bool EnableAllWeather()
        {
            try
            {
                var plugin = GetWeathermanPlugin();
                var timeAllowedZones = (HashSet<ushort>)plugin.GetType().GetField("weatherAllowedZones", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(plugin);
                foreach (var z in Svc.Data.GetExcelSheet<TerritoryType>())
                {
                    timeAllowedZones.Add((ushort)z.RowId);
                }
                return true;
            }
            catch (Exception e)
            {
                //PluginLog.Error($"{e.Message}\n{e.StackTrace}");
            }
            return false;
        }

        internal Dictionary<byte, string> GetWeathers()
        {
            try
            {
                var plugin = GetWeathermanPlugin();
                return (Dictionary<byte, string>)plugin.GetType().GetField("weathers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(plugin);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        IDalamudPlugin GetWeathermanPlugin()
        {
            try
            {
                var pluginManager = Svc.PluginInterface.GetType().Assembly.
                    GetType("Dalamud.Service`1", true).MakeGenericType(Svc.PluginInterface.GetType().Assembly.GetType("Dalamud.Plugin.Internal.PluginManager", true)).
                    GetMethod("Get").Invoke(null, BindingFlags.Default, null, new object[] { }, null);
                var installedPlugins = (System.Collections.IList)pluginManager.GetType().GetProperty("InstalledPlugins").GetValue(pluginManager);

                foreach (var t in installedPlugins)
                {
                    if ((string)t.GetType().GetProperty("Name").GetValue(t) == "Weatherman")
                    {
                        var type = t.GetType().Name == "LocalDevPlugin" ? t.GetType().BaseType : t.GetType();
                        var plugin = (IDalamudPlugin)type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(t);
                        if ((bool)plugin.GetType().GetField("Init", BindingFlags.Static | BindingFlags.NonPublic).GetValue(plugin))
                        {
                            return plugin;
                        }
                        else
                        {
                            throw new Exception("Weatherman is not initialized");
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                PluginLog.Error("Can't find Weatherman plugin: " + e.Message);
                PluginLog.Error(e.StackTrace);
                return null;
            }
        }
    }
}
