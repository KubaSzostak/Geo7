

#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Bricscad.Windows;
#endif


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;



namespace Geo7.Tools
{
    

    public class BlockImpTextPresenter : PointTextStorageReader<XyzPoint>
    {

        public BlockImpTextPresenter(TextLinesReader storage)
        {
            var blNames = new List<string>(Geo7App.DefPointBlockNames);
            var existingBlNames = Ac.GetBlockNames(false, true);
            foreach (var blName in existingBlNames)
            {
                if (!blNames.Contains(blName))
                    blNames.Add(blName);
            }
            
            this.BlockNames = blNames; // WinForms BindingSource accepts only IList or IListSource
            this.BlockName = this.BlockNames.FirstOrDefault();

            this.Storage = storage;
            this.FileName = System.IO.Path.GetFileName(Storage.Path);
            this.SampleLine = storage.GetSampleLine();

            OnSettingsChanged();
        }

        public TextLinesReader Storage { get; private set; }

        public string FileName { get; private set; }

        public List<string> BlockNames { get; private set; }

        private string _blockName;
        public string BlockName
        {
            get { return _blockName; }
            set
            {
                if (OnPropertyChanged(ref _blockName, value, nameof(BlockName)))
                {
                    this.BlockHAttr = GetBlockHAttr();
                }
            }
        }

        private string _blockHAttr;
        public string BlockHAttr
        {
            get { return _blockHAttr; }
            set { OnPropertyChanged(ref _blockHAttr, value, nameof(BlockHAttr)); }
        }

        private string GetBlockHAttr()
        {
            if (Geo7App.DefPointBlockNames.Contains(BlockName))
                return "H";

            using (var docLock = Ac.Doc.LockDocument())
            using (var trans = Ac.StartTransaction())
            {
                if (!trans.BlockTable.Has(this.BlockName))
                    return "";

                var blockDef = trans.GetBlockDef(BlockName);
                if (blockDef.HeightAttribute == null)
                    return "";

                return blockDef.HeightAttribute;
            }
        }


        private bool _saveThirdCoordinateAsHeightAttribute = false;
        public bool SaveThirdCoordinateAsHeightAttribute
        {
            get { return _saveThirdCoordinateAsHeightAttribute; }
            set { OnPropertyChanged(ref _saveThirdCoordinateAsHeightAttribute, value, nameof(SaveThirdCoordinateAsHeightAttribute)); }
        }

        private string _heightAttributePrecision = "[Auto]";
        public string HeightAttributePrecision
        {
            get { return _heightAttributePrecision; }
            set { OnPropertyChanged(ref _heightAttributePrecision, value, nameof(HeightAttributePrecision)); }
        }

        public string[] HeightAttributePrecisions { get; } = new string[] { "[Auto]", "0", "0.0", "0.00", "0.000", "0.0000" };

        

        private void AddPointsToDrawing(ICollection<XyzPoint> points)
        {
            AcDwgDb.ImportMissedBlockDef(this.BlockName, Geo7App.Geo7Dwg);
            
            using (var docLock = Ac.Doc.LockDocument())
            {
                using (var trans = Ac.StartTransaction())
                {
                    var blockScale = trans.GetSavedBlockScale(BlockName);
                    var blockDef = trans.GetBlockDef(this.BlockName);
                    Ac.InitProgress(AppServices.Strings.LoadingPoints, points.Count);
                    
                    foreach (var pt in points)
                    {
                        // Do not change X,Y coordinates - it was already done through this.ReadPoints(this.Storage, points);
                        var blockPos = pt.ToPoint3d(this.IgnoreThirdCoordinate);
                        var blockRef = blockDef.AddBlockRef(blockPos, trans);
                        blockRef.Scale = blockScale;

                        if (blockRef.Attributes.Count > 0)
                            SetAttributes(blockRef, pt);
                        
                        //blockRef.Dispose();
                        Ac.ProgressStep();
                    }
                    Ac.ClearProgress();
                    //blockDef.Dispose();
                    trans.Commit();
                }
                //Debug.WriteLine(docLock);
            }
        }


        private void SetAttributes(AcBlockRef blockRef, XyzPoint pt)
        {
            if (blockRef.IdAttribute != null)
            {
                blockRef.IdAttribute.TextString = pt.Id;
            }

            var attrH = blockRef.HeightAttribute;
            if ((attrH != null) && (this.SaveThirdCoordinateAsHeightAttribute))
            {
                if (this.HeightAttributePrecision.StartsWith("0"))
                {
                    attrH.TextString = pt.Z.ToString(this.HeightAttributePrecision);
                }
                else
                {
                    attrH.TextString = pt.Z.ToString();
                }
            }

            var attrCode = blockRef.CodeAttribute;
            if ((attrCode != null) && (this.UseCode))
                attrCode.TextString = pt.Code;
        }

        private void ReadSamplePoints(PointRepository<XyzPoint> repository)
        {
            repository.Add(new XyzPoint() { Id = "A", X = 1, Y = 5, Z = 1.1 });
            repository.Add(new XyzPoint() { Id = "B", X = 8, Y = 2, Z = 2.22 });
            repository.Add(new XyzPoint() { Id = "C", X = 2, Y = 9, Z = 3.333 });
            repository.Add(new XyzPoint() { Id = "D", X = 8, Y = 4, Z = 4.4444 });
        }
        

        public void Import()
        {
            Ac.WriteLn(AppServices.Strings.Loading);
            var points = new PointRepository<XyzPoint>();
            this.ReadPoints(this.Storage, points);
            //this.ReadSamplePoints(points);

            AddPointsToDrawing(points);
                
            var min = points.Min.ToPoint3d();
            var max = points.Max.ToPoint3d();
            Ac.ZoomTo(min, max);

            Ac.WriteLn(AppServices.Strings.XPointsLoaded, points.Count);
        }

    }
    
}
