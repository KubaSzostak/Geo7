using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.Integration;
using Geo7.ToolPalettes;


#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Colors;
using Bricscad.ApplicationServices;
using Bricscad.Windows;
using Bricscad.EditorInput;

using AcApp = Bricscad.ApplicationServices.Application;
#endif

namespace Geo7.Tools
{

    public class BlockFindCommand : PaletteCommand
    {
        public BlockFindCommand()
        {
            this.DisplayName = AcConsts.FindBlock;
            //this.SmallImage = Resources.Find;
        }

        private static PaletteSet palletteSet;

        protected override void ExecuteCore()
        {
            // use constructor with Guid so that we can save/load user data
            if (palletteSet == null)
            {
                palletteSet = new PaletteSet(AcConsts.FindBlock);
                //palletteSet.MinimumSize = new System.Drawing.Size(300, 300);
                //ToolsPalettes.Size = new System.Drawing.Size(300, 500);
                //ToolsPalettes.Style = PaletteSetStyles.ShowTabForSingle | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
                //ToolsPalettes.KeepFocus = true;
                AddPalette(palletteSet, new BlockFindPage());
            }
            palletteSet.Visible = true;
        }
    }
}
