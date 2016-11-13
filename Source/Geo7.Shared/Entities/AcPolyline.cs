
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
    public class AcPolyline<T> : AcCurve<T> where T : Polyline
    {
        public AcPolyline(T entity, AcTransaction trans)
			: base(entity, trans)
		{
            IsOnlyLines = entity.IsOnlyLines;
            Length = entity.Length;
            NumberOfVertices = entity.NumberOfVertices;
            
            _constantWidth = entity.ConstantWidth;
            _elevation = entity.Elevation;
            _normal = entity.Normal;
            _plinegen = entity.Plinegen;
            _thickness = entity.Thickness;
        }


        public bool IsOnlyLines { get; }
        public double Length { get; }
        public int NumberOfVertices { get; private set; }

        private double _constantWidth;
        public double ConstantWidth
        {
            get { return _constantWidth; }
            set
            {
                this.AcObject.ConstantWidth = value;
                _constantWidth = value;
            }
        }

        private double _elevation;
        public double Elevation
        {
            get { return _elevation; }
            set
            {
                this.AcObject.Elevation = value;
                _elevation = value;
            }
        }

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

        private bool _plinegen;
        public bool Plinegen
        {
            get { return _plinegen; }
            set
            {
                this.AcObject.Plinegen = value;
                _plinegen = value;
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
