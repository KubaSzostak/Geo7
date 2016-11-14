using System;
using System.Collections.Generic;
using System.Text;
using Geo7;
using System.Linq;

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

namespace System
{
    
    public class AcTransaction : AcDisposable
    {
        public AcTransaction(Database db, bool lockDocument)
        {
            this.Db = db;
            if (lockDocument)
                DocLock = Ac.Doc.LockDocument();
            this.Transaction = db.TransactionManager.StartTransaction();


            this.BlockTable = this.GetObject<BlockTable>(db.BlockTableId);
            this.DimStyleTable = this.GetObject<DimStyleTable>(db.DimStyleTableId);
            this.LayerTable = this.GetObject<LayerTable>(db.LayerTableId);
            this.TextStyleTable = this.GetObject<TextStyleTable>(db.TextStyleTableId);

            // Set this.BlockTable first.
            var modelSpaceId = this.BlockTable[BlockTableRecord.ModelSpace];
            this.ModelSpace = this.GetObject<BlockTableRecord>(modelSpaceId);
            
            this.InitBlockDefs();
        }

        public Database Db { get; private set; }
        private readonly DocumentLock DocLock = null;
        public Transaction Transaction { get; private set; }

		public BlockTable BlockTable { get; private set; }
		public DimStyleTable DimStyleTable { get; private set; }
		public LayerTable LayerTable { get; private set; }
		public TextStyleTable TextStyleTable { get; private set; }
        public BlockTableRecord ModelSpace { get; private set; }

        private LayerTableRecord _currentLayer = null;
        public LayerTableRecord CurrentLayer
        {
            get
            {
                if (_currentLayer?.Id != Db.Clayer)
                    _currentLayer = this.GetObject<LayerTableRecord>(Db.Clayer);
                return _currentLayer;
            }
            set
            {
                if (Db.Clayer != value.Id)
                    Db.Clayer = value.Id;
            }
        }
        

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            
            DisposeTransactionObjects();
            this.Transaction.Dispose();
            GC.SuppressFinalize(this.Transaction);
            this.Transaction = null;

            if (DocLock != null)
                DocLock.Dispose();
        }

		public void Commit()
        {
            DisposeTransactionObjects();
            try
            {
                if ((this.Transaction != null) && (!this.Transaction.IsDisposed))
                {
                    this.Transaction.Commit();
                }

            }
            catch (Exception ex)
            {
                Ac.ShowErr(ex);
                //throw;
            };
        }


        private HashSet<DBObject> transactionObjects = new HashSet<DBObject>();

        /// <summary>
        /// https://knowledge.autodesk.com/search-result/caas/CloudHelp/cloudhelp/2015/ENU/AutoCAD-NET/files/GUID-9DFB5767-F8D6-4A88-87D6-9676C0189369-htm.html
        /// When creating new objects in .NET, you must properly free the objects from memory through 
        /// the disposal process and garbage collection. You use the Dispose method or the Using statement 
        /// to signal when an object is ready for garbage collection. The Using statement in most cases 
        /// is the preferred method, as it makes the proper calls to close and dispose of the object 
        /// when it is no longer needed.
        /// 
        /// You need to dispose of an object under the following conditions:
        /// - Always with a Transaction or DocumentLock object
        /// - Always with *newly created* database objects, objects derived from DBObject, that are being added to a transaction
        /// - Always with newly created database objects, objects derived from DBObject, that are not added to the database
        /// - Do not have to with existing database objects, objects derived from DBObject, opened with a transaction object 
        ///   and the GetObject method
        ///   
        /// Disposing in Transaction example:
        /// http://docs.autodesk.com/ACD/2014/ENU/index.html?url=files/GUID-2656E245-6EAA-41A3-ABE9-742868182821.htm,topicNumber=d30e720423
        /// </summary>
        private void DisposeTransactionObjects()
        {
            foreach (var obj in transactionObjects)
            {
                if (!obj.IsDisposed) // && !obj.AutoDelete)
                {
                    //obj.Dispose();
                    //GC.SuppressFinalize(obj);
                }
            }
            transactionObjects.Clear();            
        }

        public virtual DBObject GetObject(ObjectId id)
        {
            if (id.Equals(ObjectId.Null) || !id.IsValid)
                throw new Exception(this.GetType().Name + "GetObject(): NULL ObjectId");

            if ((this.Transaction == null) || (this.Transaction.IsDisposed))
                throw new Exception(this.GetType().Name + ".Transaction == NULL");

            var obj = this.Transaction.GetObject(id, OpenMode.ForWrite);

            //GC.SuppressFinalize(obj);
            transactionObjects.Add(obj);
            return obj;
        }



        public virtual void AddNewlyCreatedDBObject(DBObject obj, bool add)
        {
            if ((this.Transaction == null) || (this.Transaction.IsDisposed))
                throw new Exception(this.GetType().Name + ".Transaction == NULL");

            if (obj is ViewTableRecord)
            {
                System.Diagnostics.Debug.WriteLine("ViewTableRecord: ");
                Diagnostics.Debug.WriteLine(obj);
            }

            this.Transaction.AddNewlyCreatedDBObject(obj, add);
            //GC.SuppressFinalize(obj);
            transactionObjects.Add(obj);
        }


        private Dictionary<string, AcBlockDef> blockDefDict = new Dictionary<string, AcBlockDef>();

        private void InitBlockDefs()
        {
            foreach (var blockId in this.BlockTable)
            {
                var blockRec = this.GetObject<BlockTableRecord>(blockId);
                if (!blockRec.IsAnonymous && !blockRec.IsLayout)
                {
                    var blockDef = new AcBlockDef(blockRec, this);
                    blockDefDict[blockDef.Name.ToLower()] = blockDef;
                }
                else
                {
                    blockRec.Dispose();
                }
            }
        }

        public ICollection<AcBlockDef> BlockDefs
        {
            get { return blockDefDict.Values; }
        }

        public AcBlockDef GetBlockDef(string name)
        {
            name = name + "";
            if (!blockDefDict.ContainsKey(name.ToLower()))
                throw new KeyNotFoundException("Block '" + name + "' does not exists");

            return blockDefDict[name.ToLower()];
        }
    }


    public static class AcTransactionEx
	{

		public static AcTransaction StartAcTransaction(this Database db, bool lockDocument)
		{
			return new AcTransaction(db, lockDocument);
		}


		public static T GetObject<T>(this AcTransaction trans, ObjectId id) where T : DBObject
        {
            return (T)trans.GetObject(id);
        }

        public static BlockTableRecord GetBlockTableRecord(this AcTransaction trans, string blockName)
        {
            var blockTable = trans.BlockTable;
            if (!blockTable.Has(blockName))
                throw new KeyNotFoundException(blockTable.GetType().Name + " does not contain element '" + blockName + "'");

            var blockId = blockTable[blockName];
            var res = trans.GetObject<BlockTableRecord>(blockId);
            return res;
        }

        public static DrawOrderTable GetDrawOrderTable(this AcTransaction trans, BlockTableRecord blockRec)
        {
            return trans.GetObject<DrawOrderTable>(blockRec.DrawOrderTableId);
        }



        public static LayerTableRecord GetLayer(this AcTransaction trans, string name)
        {
            if (!trans.LayerTable.Has(name))
                throw new KeyNotFoundException("Layer '" + name + "' does not exists");
            var layerId = trans.LayerTable[name];
            return trans.GetObject<LayerTableRecord>(layerId);
        }

        public static LayerTableRecord GetLayer(this AcTransaction trans, ObjectId id)
        {
            if (id.ObjectClass.Name != "AcDbLayerTableRecord")
                throw new InvalidCastException("Object " + id.ObjectClass.Name + " is not a layer");
            return trans.GetObject<LayerTableRecord>(id);
        }

        public static ObjectId CreateLayer(this AcTransaction trans, string name)
        {
            if (trans.LayerTable.Has(name))
            {
                return trans.LayerTable[name];
            }

            using (var ltr = new LayerTableRecord())
            {
                ltr.Name = name;
                //ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 2);
                var res = trans.LayerTable.Add(ltr);
                trans.AddNewlyCreatedDBObject(ltr, true);
                return res;
            }
        }



        public static DBDictionary GetNOD(this AcTransaction trans)
        {
            return trans.GetObject<DBDictionary>(Ac.Db.NamedObjectsDictionaryId);
        }

        public static DBDictionary GetSubDictionary(this AcTransaction trans, DBDictionary parentDict, string subDictKey)
        {
            if (parentDict.Contains(subDictKey))
            {
                var subDictId = parentDict.GetAt(subDictKey);
                return trans.GetObject<DBDictionary>(subDictId);
            }
            else
            {
                var newDict = new DBDictionary();
                var res = parentDict.SetAt(subDictKey, newDict);
                trans.AddNewlyCreatedDBObject(newDict, true);
                return newDict;
            }
        }

        public static DBDictionary GetGeo7Dict(this AcTransaction trans, string dictName)
        {
            DBDictionary NOD = trans.GetNOD();
            DBDictionary g7Dict = trans.GetSubDictionary(NOD, "Geo7.Dictionary");
            return trans.GetSubDictionary(g7Dict, dictName);
        }

        public static IEnumerable<Line> GetAllLines(this AcTransaction trans)
        {
            return trans.GetAllEntities<Line>("AcDbLine");
        }

        public static IEnumerable<T> GetAllEntities<T>(this AcTransaction trans) where T : Entity
        {
            List<T> res = new List<T>();
            var modelSpace = trans.ModelSpace;
            foreach (var entId in modelSpace)
            {
                var ent = trans.GetObject<DBObject>(entId);
                if (ent is T)
                    res.Add(ent as T);
                else
                    ent.Dispose();
            }
            return res;
        }

        public static IEnumerable<T> GetAllEntities<T>(this AcTransaction trans, string ObjectClassName) where T : Entity
        {
            List<T> res = new List<T>();
            var modelSpace = trans.ModelSpace;
            foreach (var entId in modelSpace)
            {
                if (entId.ObjectClass.Name.ToLower() == ObjectClassName.ToLower()) // AcDbMText,AcDbText,AcDbPolyline
                {
                    var ent = trans.GetObject<T>(entId);
                    res.Add(ent);
                }
            }
            return res;
        }

        public static IEnumerable<DBText> GetAllDBText(this AcTransaction trans)
        {
            return trans.GetAllEntities<DBText>();
        }

        public static ObjectId AddEntity(this AcTransaction trans, Entity ent)
        {
            var entId = trans.ModelSpace.AppendEntity(ent); // Add the reference to ModelSpace
            trans.AddNewlyCreatedDBObject(ent, true); // Let the transaction know about it

            return entId;
        }

        public static ObjectId AddLine(this AcTransaction trans, Point3d startPnt, Point3d endPnt)
        {
            return trans.AddEntity(new Line(startPnt, endPnt));
        }


        public static ObjectId AddCircle(this AcTransaction trans, Point3d center, double radius)
        {
            Vector3d normal = new Vector3d(0.0, 0.0, 1.0);
            return trans.AddEntity(new Circle(center, normal, radius));
        }

        public static DBText AddText(this AcTransaction trans, Point3d pos, string text)
        {
			DBText txt = new DBText();
			txt.Position = pos;
			txt.TextString = text;
            trans.AddEntity(txt);

			return txt;
        }


        public static void ChangeColor(this AcTransaction trans, ObjectId entId, int color)
        {
            Entity ent = trans.GetObject<Entity>(entId);
            ent.ColorIndex = color;
        }

        public static bool IsDisplayed(this AcTransaction trans, Entity ent)
        {
            if (!ent.Visible)
                return false;

            var layer = trans.GetLayer(ent.LayerId);
            if (layer.IsHidden || layer.IsFrozen || layer.IsOff)
                return false;

            return true;
        }

        public static bool IsReadOnly(this AcTransaction trans, Entity ent)
        {
            if (!ent.IsWriteEnabled)
                return false;

            var layer = trans.GetLayer(ent.LayerId);
            if (layer.IsFrozen)
                return true;

            return false;
        }
    }



    public static class AcValuesEx
    {

        private static Xrecord GetValueRecord(this AcTransaction trans, string valName)
        {
            valName = Ac.GetValidName(valName);

            var valDict = trans.GetGeo7Dict("Values");

            if (valDict.Contains(valName))
            {
                var valId = valDict.GetAt(valName);
                return trans.GetObject<Xrecord>(valId);
            }
            else
            {
                var typedVal = new TypedValue((int)DxfCode.Text, "");
                using (var resBuff = new ResultBuffer(typedVal))
                {
                    var xRec = new Xrecord();
                    xRec.Data = resBuff;
                    valDict.SetAt(valName, xRec);
                    trans.AddNewlyCreatedDBObject(xRec, true);
                    return xRec;
                }
            }




            //ResultBuffer resbuf = new ResultBuffer(  new TypedValue((int)DxfCode.Text, "HELLO"),
            //     new TypedValue((int)DxfCode.Int16, 256),
            //     new TypedValue((int)DxfCode.Real, 25.4));
        }

        /// <summary>
        /// Gets value from 'Geo7.Dictionary.Values' dictionary. If there is no value for 'name' empty string ("") is returned.
        /// </summary>
        public static string GetValue(this AcTransaction trans, string valName)
        {
            Xrecord xRec = trans.GetValueRecord(valName);
            var val = xRec.Data.AsArray()[0];
            return val.Value.ToString();
        }

        /// <summary>
        /// Sets value in 'Geo7.Dictionary.Values' dictionary. 
        /// </summary>
        public static void SetValue(this AcTransaction trans, string valName, string value)
        {
            var typedVal = new TypedValue((int)DxfCode.Text, value);
            using (var resBuff = new ResultBuffer(typedVal))
            {
                var xRec = trans.GetValueRecord(valName);
                xRec.Data = resBuff;
            }
        }

        public static double GetSavedBlockScale(this AcTransaction trans, string blockName)
        {
            var scale = trans.GetValue(blockName + ".Scale").ToDouble();
            if (scale <= 0)
                scale = 1.0;
            return scale;
        }

        public static void SetSavedBlockScale(this AcTransaction trans, string blockName, double scale)
        {
            trans.SetValue(blockName + ".Scale", scale.ToString(Globalization.CultureInfo.InvariantCulture));
        }

    }
}
