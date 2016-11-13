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

namespace Geo7.Tools
{
	public class OrthoDistCommand : AcCommand
    {


		double TextHeight = 1.0;
		OrthoTextPos TextPos = OrthoTextPos.Auto;
		Line2d BaseLine = new Line2d();

		protected override void ExecuteCore()
		{
			this.DisplayErrormMessageBox = false;

			if (!GetBaseLine())
				return;
			if (!GetTextHeight())
				return;
			if (!GetTextPos())
				return;

			Ac.WriteLn("Ortho Text: " + Ac.ToString(this.TextHeight) + ", " + this.TextPos.ToString());

			do
			{
				if (!AddOrthoDist())
					return;

			} while (true);
		}

		private bool GetBaseLine()
		{
			PromptPointResult ptRes = Ac.Editor.GetPoint("\r\n" + AppServices.Strings.EnterStartPoint);
			if (ptRes.Status != PromptStatus.OK)
				return false;

			var startPt = ptRes.Value;

			PromptPointOptions epPrtOpts = new PromptPointOptions("\r\n" + AppServices.Strings.EnterEndPoint);
			epPrtOpts.UseBasePoint = true;
			epPrtOpts.UseDashedLine = true;
			epPrtOpts.BasePoint = startPt;
			ptRes = Ac.Editor.GetPoint(epPrtOpts);
			if (ptRes.Status != PromptStatus.OK)
				return false;

			var endPt = ptRes.Value;
			this.BaseLine = new Line2d(startPt.Convert2d(), endPt.Convert2d());
			return true;
		}

		private bool GetTextHeight()
		{
			this.TextHeight = Ac.GetValue("OrthoDist.TextHeight").ToDouble();
			if (this.TextHeight <= 0.0)
				this.TextHeight = 1.0;

			var opts = new PromptDistanceOptions(AppServices.Strings.EnterTextHeight);
			opts.DefaultValue = this.TextHeight;
			opts.Only2d = true;
			opts.UseDashedLine = true;
			opts.UseBasePoint = false;

			var res = Ac.Editor.GetDistance(opts);
			if (res.Status != PromptStatus.OK)
				return false;

			this.TextHeight = res.Value;
			return true;
		}

		private bool GetTextPos()
		{
			this.TextPos = TextToPos(Ac.GetValue("OrthoDist.TextPos"));

            // http://help.autodesk.com/view/ACD/2016/ENU/?guid=GUID-DF40DD82-FD27-43C5-B7D2-E75646B2E47E
            var opts = new PromptKeywordOptions(AppServices.Strings.EnterTextPosition);
			opts.Keywords.Add("Left");
			opts.Keywords.Add("Right");
			//opts.Keywords.Default = this.TextPos.ToString();
			opts.AppendKeywordsToMessage = true;
			//opts.AllowNone = true;
			//opts.AllowArbitraryInput = true;

			//var optsStr = opts.Keywords.GetDisplayString(false);
			var res = Ac.Editor.GetKeywords(opts);
			if (res.Status != PromptStatus.OK)
				return false;

			this.TextPos = TextToPos(res.StringResult);
			return true;
		}

		private OrthoTextPos TextToPos(string s)
		{
			switch (s)
			{
				case "Right":
					return OrthoTextPos.Right;
				default:
					return OrthoTextPos.Left;
			}
		}

		private bool AddOrthoDist()
		{
			var res = Ac.Editor.GetPoint(AppServices.Strings.EnterPoint);
			if (res.Status != PromptStatus.OK)
				return false;
			var pt = res.Value.Convert2d();
			
			var lnPt = BaseLine.GetClosestPointTo(pt, Tolerance.Global).Point;
			var lnPt3d = (new Point3d(lnPt.X, lnPt.Y, 0.0));

			var dist = BaseLine.GetDistanceTo(pt);
			var distStr = Ac.ToString(dist);

			using (var trans = Ac.StartTransaction())
			{
				var txt =new DBText();
				txt.TextString = distStr;

				if (this.TextPos == OrthoTextPos.Left)
					txt.Justify = AttachmentPoint.MiddleRight;
				else
					txt.Justify = AttachmentPoint.MiddleLeft;
				//txt.HorizontalMode = TextHorizontalMode.TextRight;

				txt.Rotation = BaseLine.Direction.Angle - (Math.PI / 2.0);
				txt.Height = this.TextHeight;
				txt.Position = lnPt3d;
				txt.AlignmentPoint = lnPt3d; // Must be set if Justify is changed

				trans.AddEntity(txt);
				trans.Commit();
				Ac.Write(distStr);
			}

			return true;
		}
	}

	public enum OrthoTextPos
	{
		Auto,
		Left,
		Right
	}
}
