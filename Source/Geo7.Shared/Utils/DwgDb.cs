using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using AcApp = Bricscad.ApplicationServices.Application;
#endif

namespace System
{
    
    public class AcDwgDb
    {

        public static Database Open(string dwgFilePath, FileShare fileShare)
        {
            if (!File.Exists(dwgFilePath))
                throw new FileNotFoundException("File not found: '" + dwgFilePath + "'", dwgFilePath);

            var db = new Database(false, true);
            db.ReadDwgFile(dwgFilePath, fileShare, true, "");

            return db;
        }

        public static Database OpenReadonly(string dwgFilePath)
        {
            return Open(dwgFilePath, FileShare.Read);
        }

        public static List<string> GetBlockNames(string dwgFile, bool onlyBlocksWithAttributes)
        {
            using (var db = OpenReadonly(dwgFile))
            using (var trans = db.StartAcTransaction(false))
            {
                var blocks = trans.BlockDefs.AsEnumerable();
                if (onlyBlocksWithAttributes)
                    blocks = blocks.Where(b => b.HasAttributes);
                return blocks.OrderBy(b=>b.Name).Select(b => b.Name).ToList();
            }
        }


        private static ObjectId ImportMissedRecord(SymbolTable srcTable, string key, SymbolTable destTable)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("ImportMissedRecord(" + srcTable.GetType().Name + ".key)");

            if (destTable.Has(key))  // It is not missed 
                return destTable[key];

            if (!srcTable.Has(key))
                throw new KeyNotFoundException("'" + key + "' not found in " + srcTable.GetType().Name);

            
            Ac.WriteLn("Updating " + destTable.GetType().Name +  "...  ");    // Updating BlockTable...

            using (var mapping = new IdMapping())
            using (var sourceIds = new ObjectIdCollection())
            {
                sourceIds.Add(srcTable[key]);

                srcTable.Database.WblockCloneObjects(sourceIds, destTable.ObjectId, mapping, DuplicateRecordCloning.Ignore, false);
                Ac.Write(string.Format("'{0}' added.", key));                // G7-FixedPoint added.                

                return destTable[key];
            }
        }

        public static ObjectId ImportMissedBlockDef(string blockName, string srcDwgFile)
        {
            using (var dwgDb = AcDwgDb.OpenReadonly(srcDwgFile))
            using (var dwgTrans = dwgDb.TransactionManager.StartTransaction())
            using (var acTrans = Ac.Db.TransactionManager.StartTransaction())
            {
                var dwgTable = (SymbolTable)dwgTrans.GetObject(dwgDb.BlockTableId, OpenMode.ForRead);
                var acTable = (SymbolTable)acTrans.GetObject(Ac.Db.BlockTableId, OpenMode.ForWrite);

                var blockId = ImportMissedRecord(dwgTable, blockName, acTable);
                using (var blockTableRec = (BlockTableRecord)acTrans.GetObject(blockId, OpenMode.ForWrite))
                {
                    SendHatchesToBack(acTrans, blockTableRec);
                }                    

                acTrans.Commit();
                return blockId;
            }
        }

        public static ObjectId ImportMissedDimStyle(string name, string srcDwgFile)
        {
            using (var dwgDb = AcDwgDb.OpenReadonly(srcDwgFile))
            using (var dwgTrans = dwgDb.TransactionManager.StartTransaction())
            using (var acTrans = Ac.Db.TransactionManager.StartTransaction())
            {
                var dwgTable = (SymbolTable)dwgTrans.GetObject(dwgDb.DimStyleTableId, OpenMode.ForRead);
                var acTable = (SymbolTable)acTrans.GetObject(Ac.Db.DimStyleTableId, OpenMode.ForWrite);               

                var res = ImportMissedRecord(dwgTable, name, acTable);
                acTrans.Commit();
                return res;
            }
        }

        public static ObjectId ImportMissedTextStyle(string name, string srcDwgFile)
        {
            using (var dwgDb = AcDwgDb.OpenReadonly(srcDwgFile))
            using (var dwgTrans = dwgDb.TransactionManager.StartTransaction())
            using (var acTrans = Ac.Db.TransactionManager.StartTransaction())
            {
                var dwgTable = (SymbolTable)dwgTrans.GetObject(dwgDb.TextStyleTableId, OpenMode.ForRead);
                var acTable = (SymbolTable)acTrans.GetObject(Ac.Db.TextStyleTableId, OpenMode.ForWrite);

                var res = ImportMissedRecord(dwgTable, name, acTable);
                acTrans.Commit();
                return res;
            }
        }

        private static void SendHatchesToBack(Transaction trans, BlockTableRecord blockTable)
        {
            using (var hatchIds = new ObjectIdCollection())
            {
                foreach (ObjectId id in blockTable)
                {
                    if (id.ObjectClass.Name == "AcDbHatch")
                        hatchIds.Add(id);

                    /*
                    //Use it to open the current object! 
                    using (var ent = trans.GetObject(id, OpenMode.ForRead))
                    {
                        if (ent is Hatch)
                        {
                            Ac.WriteLn(ent.ObjectId.ObjectClass.DxfName);
                            hatchIds.Add(ent.ObjectId);
                        }
                    }
                    */
                }
                using (var drawOrderTable = (DrawOrderTable)trans.GetObject(blockTable.DrawOrderTableId, OpenMode.ForWrite))
                {
                    drawOrderTable.MoveToBottom(hatchIds);
                }
            }
        }


    }

    

}
