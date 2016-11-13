
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
    public class AcCurve<T> : AcEntity<T> where T:Curve
    {

        public AcCurve(T entity, AcTransaction trans)
			: base(entity, trans)
		{
            Area = entity.Area;
            Closed = entity.Closed;
            IsPeriodic = entity.IsPeriodic;
            _startPoint = entity.StartPoint;
            _endPoint = entity.EndPoint;
        }

        public double Area { get; private set; }
        public bool Closed { get; private set; }
        public virtual bool IsPeriodic { get; private set; }


        private Point3d _startPoint;
        public Point3d StartPoint 
        {
            get { return _startPoint; }
            set
            {
                this.AcObject.StartPoint = value;
                _startPoint = value;
            }
        }

        private Point3d _endPoint;
        public virtual Point3d EndPoint
        {
            get { return _endPoint; }
            set
            {
                this.AcObject.EndPoint = value;
                _endPoint = value;
            }
        }
    }


}
