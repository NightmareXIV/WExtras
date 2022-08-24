using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WExtras
{
    internal unsafe class ConfigGui : Window
    {
        WExtras p;

        public ConfigGui(WExtras p) : base("WExtras configuration", ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.p = p;
        }

        public override void OnClose()
        {
            Svc.PluginInterface.SavePluginConfig(p.config);
            base.OnClose();
        }

        public override void Draw()
        {
            try
            {
                if (ImGui.Button("Enable all zones for time adjust"))
                {
                    p.EnableAllTime();
                }
                if (ImGui.Button("Enable all zones for weather adjust"))
                {
                    p.EnableAllWeather();
                }
                ImGui.Checkbox("Auto-enable all zones for time adjust", ref p.config.AutoUnlockTimes);
                ImGui.Checkbox("Auto-enable all zones for weather adjust", ref p.config.AutoUnlockWeathers);
                var w = p.GetWeathers();
                if(p != null)
                {
                    if (ImGui.BeginCombo("##setweather", "Select weather..."))
                    {
                        foreach (var x in p.GetWeathers())
                        {
                            if (ImGui.Selectable($"{x.Key} / {x.Value}"))
                            {
                                p.SetWeather(x.Key);
                            }
                        }
                        ImGui.EndCombo();
                    }

                    if (ImGui.BeginCombo("##setweathert", "Override true weather value..."))
                    {
                        foreach (var x in p.GetWeathers())
                        {
                            if (ImGui.Selectable($"{x.Key} / {x.Value}"))
                            {
                                *p.TrueWeather = x.Key;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
            }
            catch(Exception ex)
            {
                PluginLog.Error(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
