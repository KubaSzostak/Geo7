
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
using System.ComponentModel;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	public class AcText<T> : AcEntity<T> where T : DBText
	{
		public AcText(T entity, AcTransaction trans)
			: base(entity, trans)
		{
			mPosition = entity.Position;
			mRotation = entity.Rotation;
			mTextString = entity.TextString;
			mHorizontalMode = entity.HorizontalMode;
			mVerticalMode = entity.VerticalMode;
		}


		private Point3d mPosition;
		public Point3d Position
		{
			get { return mPosition; }
			set
			{
				this.AcObject.Position = value;
				mPosition = value;
			}
		}

		public double mRotation;
		public double Rotation
		{
			get { return mRotation; }
			set
			{
				this.AcObject.Rotation = value;
				mRotation = value;
			}
		}

		private string mTextString;
		public string TextString
		{
			get { return mTextString; }
			set
			{
				this.AcObject.TextString = value;
				mTextString = value;
			}
		}


		private TextHorizontalMode mHorizontalMode;
		public TextHorizontalMode HorizontalMode
		{
			get { return mHorizontalMode; }
			set
			{
				this.AcObject.HorizontalMode = value;
				mHorizontalMode = value;
			}
		}

		private TextVerticalMode mVerticalMode;
		public TextVerticalMode VerticalMode
		{
			get { return mVerticalMode; }
			set
			{
				this.AcObject.VerticalMode = value;
				mVerticalMode = value;
			}
		}
	}

    public class AcText : AcText<DBText>
    {
        public AcText(DBText entity, AcTransaction trans)
            : base(entity, trans)
        {
        }
    }
}
