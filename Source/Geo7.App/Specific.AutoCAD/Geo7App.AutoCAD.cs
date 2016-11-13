using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geo7
{
    partial class Geo7ToolsApp
    {
        partial void InternalInit()
        {
            AppLog.Add("Initialize(): Ribbon.RibbonUtils.Init()...");
            RibbonUtils.Init();
        }
        partial void ShowRibbon()
        {
            RibbonUtils.ShowGeo7Ribbon();
        }
    }
}
