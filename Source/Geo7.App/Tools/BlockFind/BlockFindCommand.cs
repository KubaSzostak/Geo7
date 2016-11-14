

#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Bricscad.Windows;
#endif


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.Integration;
using Geo7.ToolPalettes;


namespace Geo7.Tools
{

    public class BlockFindCommand : PaletteCommand
    {
        public BlockFindCommand()
        {
            this.DisplayName = AppServices.Strings.FindBlock;
            //this.SmallImage = Resources.Find;
        }

        private static PaletteSet palletteSet;

        protected override void ExecuteCore()
        {
            // use constructor with Guid so that we can save/load user data
            if (palletteSet == null)
            {
                palletteSet = new PaletteSet(AppServices.Strings.FindBlock);
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
