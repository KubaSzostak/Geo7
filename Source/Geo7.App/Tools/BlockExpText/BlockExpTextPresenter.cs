
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

    public class BlockExpTextPresenter : PointTextStorageWriter<XyzPoint>
    {
        public BlockExpTextPresenter(List<AcBlockRef> blockRefs)
        {
            if (blockRefs?.Count < 1)
            {
                throw new ArgumentException(this.GetType().Name + " initialization failed: empty AcBlockRef list");
            }

            _blockRefs = blockRefs;
            this.UseCode = false; // To export code use this.UseAdditionalFields instead
            this.SampleData = ToPoint(blockRefs.First());

            this.AddHeader(Ac.Db.Filename);
            //this.AddHeader(Ac.Doc.Name);
            this.AddHeader(DateTime.Now.ToShortDateString());

            UpdateAdditionalFieldNames(blockRefs.FirstOrDefault());
        }

        private List<AcBlockRef> _blockRefs;

        public int BlockCount { get { return _blockRefs.Count; } }


        private void UpdateAdditionalFieldNames(AcBlockRef sampleBlockRef)
        {
            this.AdditionalFieldNames.Clear();
            this.AdditionalFieldNames.Add("Layer");
            this.AdditionalFieldNames.Add("Block");

            foreach (var attr in sampleBlockRef.Attributes)
            {
                this.AdditionalFieldNames.Add("Attr_" + attr.Tag);
            }
        }

        private bool _useHeightAttribute;
        public bool UseHeightAttribute
        {
            get { return _useHeightAttribute; }
            set { OnPropertyChanged(ref _useHeightAttribute, value, nameof(UseHeightAttribute)); }
        }


        public string ValidIdAttributes { get { return Ac.IdAttributeTags.Join(", "); } }
        public string ValidHeightAttributes { get { return Ac.HeightAttributeTags.Join(", "); } }


        protected override void OnSampleDataChanged()
        {
            base.OnSampleDataChanged();
        }

        
        private XyzPoint ToPoint(AcBlockRef block)
        {
            var pt = new XyzPoint();

            pt.X = block.Position.X;
            pt.Y = block.Position.Y;
            pt.Z = block.Position.Z;

            // Do not change coordinates order, this will be done by PointTextStorageWriter.GetPointFields()
            //if (this.ChangeCoordinatesOrder)
            //{
            //    pt.X = block.Position.Y;
            //    pt.Y = block.Position.X;
            //}

            if (block.IdAttribute != null)
                pt.Id = block.IdAttribute.TextString;

            if (block.CodeAttribute != null)
                pt.Code = block.CodeAttribute.TextString;

            if (this.UseHeightAttribute)
            {
                double hAttrVal = 0.0;
                if (block.HeightAttribute != null)
                {
                    var hAttr = block.HeightAttribute.TextString;
                    hAttr.TryParse(ref hAttrVal);
                }
                pt.Z = hAttrVal;
            }

            pt.AdditionalFields.Clear();
            pt.AdditionalFields.Add(block.Layer);
            pt.AdditionalFields.Add(block.Name);
            foreach (var attr in block.Attributes)
            {
                pt.AdditionalFields.Add(attr.TextString);
            }

            pt.Coord1.Precision = CoordinatesPrecision;
            pt.Coord2.Precision = CoordinatesPrecision;
            pt.Coord3.Precision = ThirdCoordinatePrecision;

            return pt;

        }

        private PointRepository<XyzPoint> GetPoints()
        {
            var points = new PointRepository<XyzPoint>();

            foreach (var bl in _blockRefs)
            {
                points.Add(ToPoint(bl));
            }
            return points;
        }

        public override IList<string> GetPointFields(XyzPoint point)
        {
            return base.GetPointFields(point);
        }

        private void Export()
        {
            var dlg = AppServices.SaveFileDialog;
            var storage = dlg.ShowTextLinesWritersDialog(Ac.GetLastFileName("points"));
            if (storage == null)
                return;

            using (storage)
            {
                Ac.SetLastFileName("points", dlg.FilePath);
                base.WritePoints(storage, this.GetPoints());
            }          
        }

        public ICommand ExportCommand
        {
            get { return new DelegateCommand(Export); }
        }
    }


}
