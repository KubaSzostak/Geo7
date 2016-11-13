#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo7.Windows;

namespace Geo7.Tools
{
    public class PtsLnDistCommand : AcCommand
    {
        ObjectId DimStyleId;
        ObjectId LineId;

        protected override void ExecuteCore()
        {
            this.DisplayErrormMessageBox = false;
            this.DimStyleId = AcDwgDb.ImportMissedDimStyle("G7-Slope", Geo7App.Geo7Dwg);

            LineId = Ac.Editor.GetLine("\r\n" + AppServices.Strings.SelectLine + ": ");

            if (LineId.IsValid)
            {
                Ac.WriteLn(AppServices.Strings.SelectBlock);
                var selWnd = new BlockSelectWindow(OnBlocksSelected);
                Ac.ShowModal(selWnd); 
            }
        }

        private void OnBlocksSelected(List<AcBlockRef> blockRefs)
        {
            using (var trans = Ac.StartTransaction())
            {
                var ln = trans.GetObject<Curve>(this.LineId);
                foreach (var bl in blockRefs)
                {
                    var lnPt = ln.GetClosestPointTo(bl.Position, false);
                    AlignedDimension dim = new AlignedDimension(bl.Position, lnPt, lnPt, null, this.DimStyleId);
                    dim.LayerId = trans.CurrentLayer.Id;
                    
                    trans.AddEntity(dim);
                }
                trans.Commit();
            }
        }

    }


}