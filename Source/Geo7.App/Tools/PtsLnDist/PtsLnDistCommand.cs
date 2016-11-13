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
            this.DimStyleId = AcDwgDb.ImportMissedDimStyle("G7-Diff", Geo7App.Geo7Dwg);

            LineId = Ac.Editor.GetPolyline("\r\n" + AppServices.Strings.SelectLine + ": ");

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
                var ln = trans.GetObject<Polyline>(this.LineId);
                if ((ln.StartPoint.Z != 0.0) || (ln.EndPoint.Z != 0.0))
                    Ac.WriteLn("WARNING: Line is not on plane (Elevation != 0.0; Z != 0)");

                var ln2d = ln.ConvertTo(false);
                //ln2d.Elevation = 0;
                //foreach (Vertex2d pt in ln2d)
                //{
                //    Ac.WriteLn(pt.Position.ToString());
                //}

                foreach (var bl in blockRefs)
                {
                    var blPt2d = new Point3d(bl.Position.X, bl.Position.Y, 0.0);
                    var lnPt = ln2d.GetClosestPointTo(blPt2d, false);
                    lnPt = new Point3d(lnPt.X, lnPt.Y, 0.0);

                    AlignedDimension dim = new AlignedDimension(blPt2d, lnPt, lnPt, null, this.DimStyleId);
                    dim.LayerId = trans.CurrentLayer.Id;
                    
                    trans.AddEntity(dim);
                }
                ln2d.Dispose();
                trans.Commit();
            }
        }

    }


}