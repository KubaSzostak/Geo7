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

}
