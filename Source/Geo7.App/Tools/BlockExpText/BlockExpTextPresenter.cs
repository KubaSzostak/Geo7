using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Teigha.DatabaseServices;

namespace Geo7.Tools
{

    public class BlockExpTextPresenter : PointTextStorageWriter<XyzPoint>
    {
        public BlockExpTextPresenter(System.Windows.Window wnd)
        {
            this.BlockNames = Ac.GetBlockNames(true, true).ToList(); // WinForms BindingSource accepts only IList or IListSource
            this.BlockName = this.BlockNames.FirstOrDefault();
            this.UseCode = false; // Use this.UseAdditionalFields instead

            this.AddHeader(Ac.Db.Filename);
            //this.AddHeader(Ac.Doc.Name);
            this.AddHeader(DateTime.Now.ToShortDateString());

            _window = wnd;
        }

        private System.Windows.Window _window;

        private bool _useHeightAttribute;
        public bool UseHeightAttribute
        {
            get { return _useHeightAttribute; }
            set { OnPropertyChanged(ref _useHeightAttribute, value, nameof(UseHeightAttribute)); }
        }


        public string ValidIdAttributes { get { return Ac.IdAttributeTags.Join(", "); } }
        public string ValidHeightAttributes { get { return Ac.HeightAttributeTags.Join(", "); } }

        public bool HasSelectedBlocks { get { return this.SelectedIds.Count > 0; } }
        public int SelectedBlockCount { get { return this.SelectedIds.Count; } }
        
        private HashSet<ObjectId> SelectedIds = new HashSet<ObjectId>();


        public void AddSelectedIds(IEnumerable<ObjectId> ids)
        {
            this.SelectedIds.AddItems(ids);
            this.OnPropertyChanged(nameof(HasSelectedBlocks));
            this.OnPropertyChanged(nameof(SelectedBlockCount));
        }

        private void ClearSelectedIds()
        {
            this.SelectedIds.Clear();
            this.OnPropertyChanged(nameof(HasSelectedBlocks));
            this.OnPropertyChanged(nameof(SelectedBlockCount));

        }

        public ICommand ClearSeclectedBlocksCommand
        {
            get { return new DelegateCommand(ClearSelectedIds, ()=> { return this.HasSelectedBlocks; }); }
        }

        public void SelectBlocks(System.Windows.Forms.Form modalForm)
        {
            using (var docLock = Ac.Doc.LockDocument())
            using (var ui = Ac.StartUserInteraction(modalForm))
            {
                var selectedBlocks = Ac.SelectBlocks(this.BlockName);
                AddSelectedIds(selectedBlocks);
                ui.End();
            }
        }

        public void SelectAllBlocks()
        {
            using (var docLoc = Ac.Doc.LockDocument())
            {
                var selectedBlocks = Ac.SelectAllBlocks(this.BlockName);
                AddSelectedIds(selectedBlocks);
            }
        }

        public ICommand SelectAllBlocksCommand
        {
            get { return new DelegateCommand(SelectAllBlocks); }
        }
        

        private List<AcBlockRef> GetSelectedBlocks()
        {
            var res = new List<AcBlockRef>();
            using (var trans = Ac.StartTransaction())
            {
                foreach (var id in SelectedIds)
                {
                    var block = new AcBlockRef(id, trans);
                    res.Add(block);
                }
            }

            return res;
        }

        public List<string> BlockNames { get; private set; }

        private string _blockName;
        public string BlockName
        {
            get { return _blockName; }
            set
            {
                if (OnPropertyChanged(ref _blockName, value, nameof(this.BlockName)))
                {
                    this.AdditionalFieldNames.Clear();
                    this.AdditionalFieldNames.Add("Layer");
                    this.AdditionalFieldNames.Add("Block");


                    using (var trans = Ac.StartTransaction())
                    {
                        var blockDef = trans.GetBlockDef(this.BlockName);

                        this.SampleData.AdditionalFields.Clear();
                        this.SampleData.AdditionalFields.Add("DefPoints"); 
                        this.SampleData.AdditionalFields.Add(this.BlockName);

                        foreach (var attr in blockDef.Attributes)
                        {
                            this.AdditionalFieldNames.Add("Attr_" + attr.Tag);
                            this.SampleData.AdditionalFields.Add(attr.TextString);
                        }
                    }
                    OnSampleDataChanged();
                }
            }
        }

        protected override void OnSampleDataChanged()
        {
            base.OnSampleDataChanged();
        }


        string GetLineFormat(AcBlockRef br)
        {
            var res = "Id,X,Y,Z";
            /*
            if (this.chAddAllAttributes.IsChecked.GetValueOrDefault(false))
            {
                foreach (var attr in br.Attributes)
                {
                    res = res + "," + attr.Tag;
                }
                res += ",Block,Layer";
            }
            */
            return res;
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
            var blocks = GetSelectedBlocks();

            foreach (var bl in blocks)
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
            _window.Close();

            var storage = AppServices.SaveFileDialog.ShowTextLinesWritersDialog(Ac.GetLastFileName("points"));
            if (storage == null)
                return;

            using (storage)
            {
                base.WritePoints(storage, this.GetPoints());
            }          
        }

        public ICommand ExportCommand
        {
            get { return new DelegateCommand(Export, () => HasSelectedBlocks); }
        }

        private void SelectBlocks()
        {
            using (var docLock = Ac.Doc.LockDocument())
            using (var ui = Ac.StartUserInteraction(_window))
            {
                var selectedBlocks = Ac.SelectBlocks(BlockName);
                AddSelectedIds(selectedBlocks);
                ui.End();
            }
        }

        public ICommand SelectBlocksCommand
        {
            get { return new DelegateCommand(SelectBlocks); }
        }
    }


}
