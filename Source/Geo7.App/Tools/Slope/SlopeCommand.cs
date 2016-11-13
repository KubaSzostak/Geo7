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

//[assembly: CommandClass(typeof(Geo7.AutoCAD.Commands.PromptsTest))]
//[assembly: CommandClass(typeof(Geo7.AutoCAD.Commands.TestEntityJig))]

namespace Geo7.Tools
{
	public class SlopeCommand : AcCommand
    {
		protected override void ExecuteCore()
		{
			this.DisplayErrormMessageBox = false;
			this.DimStyleId = AcDwgDb.ImportMissedDimStyle("G7-Slope", Geo7App.Geo7Dwg);

			PromptPointOptions epPrtOpts = new PromptPointOptions("\r\n" + AcConsts.EnterEndPoint);
			epPrtOpts.UseBasePoint = true;
			epPrtOpts.UseDashedLine = true;

			do
			{
				PromptPointResult ptRes = Ac.Editor.GetPoint("\r\n" + AcConsts.EnterStartPoint);
				if (ptRes.Status != PromptStatus.OK)
					return;
				Point3d startPnt = ptRes.Value;

				epPrtOpts.BasePoint = startPnt;
				ptRes = Ac.Editor.GetPoint(epPrtOpts);
				if (ptRes.Status != PromptStatus.OK)
					return;
				Point3d endPnt = ptRes.Value;

				AddSlope(startPnt, endPnt);

			} while (true);
		}

		ObjectId DimStyleId;

		private void AddSlope(Point3d startPnt, Point3d endPnt)
		{
			var dx = endPnt.X - startPnt.X;
			var dy = endPnt.Y - startPnt.Y;
			var dz = endPnt.Z - startPnt.Z;
			Vector2d v = new Vector2d(dx, dy);
			var slope = dz / v.Length;
			
			using (var trans = Ac.StartTransaction())
			{
				var dimText = (slope * 100).ToString("0.0") + @"%";
				AlignedDimension dim = new AlignedDimension(startPnt, endPnt, startPnt, dimText, this.DimStyleId);
				//dim.XLine1Point = startPnt;
				//dim.XLine2Point = endPnt;
				//dim.DimLinePoint = startPnt;
				//dim.Suffix = @"%";
				//dim.DimensionText = 
				//dim.DimensionStyle = 

				trans.AddEntity(dim);
				trans.Commit();
			}
		}

	}

	
}
