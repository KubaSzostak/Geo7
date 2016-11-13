
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
using System.Text;

namespace System
{
    public class AcLine<T> : AcCurve<T> where T : Line
    {
        public AcLine(T entity, AcTransaction trans)
			: base(entity, trans)
		{
            Angle = entity.Angle;
            Delta = entity.Delta;
            Length = entity.Length;

            _normal = entity.Normal;
            _thickness = entity.Thickness;
        }


        public double Angle { get; private set; }
        public Vector3d Delta { get; private set; }
        public double Length { get; private set; }

        private Vector3d _normal;
        public Vector3d Normal
        {
            get { return _normal; }
            set
            {
                this.AcObject.Normal = value;
                _normal = value;
            }
        }

        private double _thickness;
        public double Thickness
        {
            get { return _thickness; }
            set
            {
                this.AcObject.Thickness = value;
                _thickness = value;
            }
        }
    }
}
