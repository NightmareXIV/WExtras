using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WExtras
{
    internal class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public bool AutoUnlockWeathers = false;
        public bool AutoUnlockTimes = false;
    }
}
