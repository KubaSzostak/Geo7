using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Linq;
using Geo7;


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

namespace System
{


    public static class AcPolylineEx
    {
        public static void AddVertex(this Polyline polyline, Point3d point)
        {
            //polyline.NumberOfVertices
            polyline.AddVertex(point.Convert2d());
        }

        public static void AddVertex(this Polyline polyline, Point2d point)
        {
            //polyline.NumberOfVertices
            polyline.AddVertexAt(polyline.NumberOfVertices, point, 0, 0, 0);
        }

        public static List<Point2d> GetPoints2d(this Polyline polyline)
        {
            List<Point2d> res = new List<Point2d>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
			{
			    res.Add(polyline.GetPoint2dAt(i));
			}
            return res;
        }

        public static List<Point3d> GetPoints3d(this Polyline polyline)
        {
            List<Point3d> res = new List<Point3d>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
			{
			    res.Add(polyline.GetPoint3dAt(i));
			}
            return res;
        }

        public static bool IsInside(this Polyline polyline, Point3d point)
        {
            var ray = new Ray();
            ray.BasePoint = point;
            ray.SecondPoint = new Point3d(point.X+100, point.Y, point.Z);
            Point3dCollection pntColl = new Point3dCollection();
            //polyline.IntersectWith(ray, Intersect.ExtendThis, pntColl, 0, 0);
            polyline.IntersectWith(ray, Intersect.ExtendThis, pntColl, IntPtr.Zero, IntPtr.Zero);
            
            return (pntColl.Count % 2) != 0;
        }


    }

    public static class AcPointEx
    {
        public static bool Equals(this Point2d thisPoint, Point2d anotherPoint)
        {
            return thisPoint.Id() == anotherPoint.Id();
        }

        public static string Id(this Point2d p, string precisionFormat)
        {
            return "[" + p.X.ToString(precisionFormat) + ";" + p.Y.ToString(precisionFormat) + "]";
        }

        public static string Id(this Point2d p)
        {
            return p.Id(Ac.LinearPrecisionFormat);
        } 

        public static Point2d Convert2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);

        }

        public static Point3d ToPoint3d(this XyzPoint pt, bool ignoreZ = false)
        {
            if (ignoreZ)
                return new Point3d(pt.X, pt.Y, 0.0);
            else
                return new Point3d(pt.X, pt.Y, pt.Z);
        }

        public static Point3d ToPoint3dPlanar(this Point3d pt)
        {
            return new Point3d(pt.X, pt.Y, 0.0);
        }

        public static XyzPoint ToXyzPoint(this Point3d pt)
        {
            return new XyzPoint() { X = pt.X, Y = pt.Y, Z = pt.Y };
        }

        public static XyzPoint ToXyzPoint(this Point2d pt)
        {
            return new XyzPoint() { X = pt.X, Y = pt.Y};
        }


    }

    public static class BlockEx
    {
    }

    public static class EditorEx
    {
        public static bool IsOK(this PromptResult prompt)
        {
            return prompt.Status == PromptStatus.OK;
        }

        public static bool IsCancel(this PromptResult prompt)
        {
            return prompt.Status == PromptStatus.Cancel;
        }

        public static PromptPointOptions GetPromptPointOptions(this Editor ed, string prompt, Point3d basePoint, bool UseDashedLine)
        {
            var res =  new PromptPointOptions(prompt);
            res.BasePoint = basePoint;
            res.UseBasePoint = true;
            res.UseDashedLine = UseDashedLine;

            return res;
        }


        public static SelectionFilter GetBlockSelectionFilter(this Editor ed, params string[] blockNames)
        {
            var values = new TypedValue[]
            {
                Ac.GetTypedValue(DxfCode.Start, "INSERT" ),
                Ac.GetTypedValue(DxfCode.BlockName, blockNames.Join(",") )
            };
            return new SelectionFilter(values);
        }

        public static AcObjectIds GetBlocks(this Editor ed, params string[] blockNames)
        {
            //PromptEntityOptions peo = new PromptEntityOptions("Select blocks: ");
            //var res = Editor.GetEntity(peo);

            var res = new AcObjectIds();

            var filter = GetBlockSelectionFilter(ed, blockNames);
            PromptSelectionOptions promptOpt = new PromptSelectionOptions();
            promptOpt.MessageForAdding = "Select blocks:";

            PromptSelectionResult selRes = Ac.Editor.GetSelection(promptOpt, filter);
            if (selRes.Status == PromptStatus.OK)
                res.AddItems(selRes.Value.GetObjectIds());
            return res;
        }

        public static AcObjectIds GetAllBlocks(this Editor ed, string blockName)
        {
            var res = new AcObjectIds();
            var filter = GetBlockSelectionFilter(ed, blockName);
            var selRes = Ac.Editor.SelectAll(filter);
            if (selRes.Status == PromptStatus.OK)
                res.AddItems(selRes.Value.GetObjectIds());
            return res;
        }

        public static ObjectId GetLine(this Editor ed, string prompt)
        {
            var res = ObjectId.Null;

            var promptOpts = new PromptEntityOptions(prompt);
            promptOpts.SetRejectMessage(prompt);
            promptOpts.AddAllowedClass(typeof(Polyline), true);
            promptOpts.AddAllowedClass(typeof(Line), true);

            var promptRes = ed.GetEntity(promptOpts);

            if (promptRes.Status == PromptStatus.OK)
                return promptRes.ObjectId;
            else
                return ObjectId.Null;
        }

        public static ObjectId GetPolyline(this Editor ed, string prompt)
        {
            var res = ObjectId.Null;

            var promptOpts = new PromptEntityOptions(prompt);
            promptOpts.SetRejectMessage(prompt);
            promptOpts.AddAllowedClass(typeof(Polyline), true);

            var promptRes = ed.GetEntity(promptOpts);

            if (promptRes.Status == PromptStatus.OK)
                return promptRes.ObjectId;
            else
                return ObjectId.Null;
        }
    }
}
