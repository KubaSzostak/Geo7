
#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;

#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Colors;
using Bricscad.ApplicationServices;
using Bricscad.Windows;
using Bricscad.EditorInput;

#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Geo7.Windows;

namespace Geo7.Tools
{

    public class BlockExpTextCommand : AcCommand
    {
        protected override void ExecuteCore()
        {
            if (Ac.GetBlockNames(true, true).Count< 1)
                throw new System.Exception(AppServices.Strings.BlocksNotFond);

            var wnd = new BlockExpTextWindow();
            wnd.ShowDialog();          
        }

    }

}
