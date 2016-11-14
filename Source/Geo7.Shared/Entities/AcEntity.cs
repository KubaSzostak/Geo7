
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

    public class AcDisposable : IDisposable
    {
        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                //DisposeAcObject();
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    public class AcDbObject //: INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;
    }
    
    public class AcDbObject<TObject> : AcDbObject  where TObject : DBObject
    {
        public AcDbObject(TObject obj, AcTransaction trans)
        {
            this.ObjectId = obj.ObjectId;
            this.acObject = obj;
        }

        public ObjectId ObjectId { get; private set; }
        
        private TObject acObject = null;
        protected TObject AcObject
        {
            get
            {
                if ((this.acObject == null) || (this.acObject.IsDisposed))
                    throw new InvalidOperationException(this.GetType().Name + ".AcObject is inaccessible.");
                return acObject;
            }
        }

        public TObject GetAcObject(AcTransaction trans)
        {
            if ((this.acObject != null) && (!this.acObject.IsDisposed))
                return this.acObject;

            return trans.GetObject<TObject>(this.ObjectId);
        }

    }



    public class AcEntity<TEntity> :AcDbObject<TEntity> where TEntity : Entity
    {
        public AcEntity(TEntity entity, AcTransaction trans)
            : base(entity, trans)
        {
            _layer = entity.Layer;
            this.Displayed = trans.IsDisplayed(entity);
        }

        private string _layer;
        public string Layer
        {
            get
            {
                return _layer;
            }
            set
            {
                this.AcObject.Layer = value;
                _layer = value;
            }
        }

        public bool Displayed { get; private set; }

        public void UpgradeOpen()
        {
            this.AcObject.UpgradeOpen();
        }
    }
}
