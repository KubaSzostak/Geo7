using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace Geo7.Tools
{


    public class BlocksSettingsPresenter : Presenter
    {
        public BlocksSettingsPresenter()
        {
            using (var trans = Ac.StartTransaction())
            {
                BlockList = trans.BlockDefs.Where(b => (b.HasAttributes) && (b.HasReferences)).ToList();
            }
            
            SelectedBlock = BlockList.FirstOrDefault();
        }

        public List<AcBlockDef> BlockList { get; private set; }

        public void Save()
        {
            //Ac.InitProgress("Saving block data...", SelectedBlock.BlockCount);            

            using (var docLock = Ac.Doc.LockDocument())
            using (var trans = Ac.StartTransaction())
            {
                var blockDef = trans.GetBlockDef(SelectedBlock.Name);
                foreach (var attr in blockDef.Attributes)
                {
                    // Update BlockDef from 'editable' property value
                    var attrPresenter = SelectedBlock.GetAttribute(attr.Tag);
                    attr.Invisible = !attrPresenter.AttrVisiblePresenter;
                }

                var blockRefs = blockDef.GetBlocks(trans);
                foreach (var blockRef in blockRefs)
                {
                    blockRef.ResetBlock();
                    blockRef.Scale = SelectedBlockScale;

                    foreach (var attr in blockRef.Attributes)
                    {
                        var attrPresenter = SelectedBlock.GetAttribute(attr.Tag);
                        attr.Invisible = !attrPresenter.AttrVisiblePresenter;
                    }

                    Ac.ProgressStep();
                }
                trans.SetSavedBlockScale(SelectedBlock.Name, SelectedBlockScale);
                trans.Commit();
            }
            
            Ac.ClearProgress();
        }

        public ICommand SaveCommand
        {
            get { return new DelegateCommand(Save, SaveCanExecute); }
        }


        private bool SaveCanExecute()
        {
            return SelectedBlock != null;
        }

        private AcBlockDef mSelectedBlock;
        public AcBlockDef SelectedBlock
        {
            get
            {
                return mSelectedBlock;
            }
            set
            {
                mSelectedBlock = value;
                if (mSelectedBlock == null)
                {
                    SelectedBlockScale = 1.0;
                    SelectedBlockRefCount = 0;
                }
                else
                {
                    using (var docLock = Ac.Doc.LockDocument())
                    using (var trans = Ac.StartTransaction())
                    {
                        SelectedBlockScale = SelectedBlock.FirstBlock(trans).Scale;
                        SelectedBlockRefCount = SelectedBlock.BlockCount;
                    }
                }
                OnPropertyChanged(nameof(SelectedBlock));
                OnPropertyChanged(nameof(SelectedBlockScale));
                OnPropertyChanged(nameof(SelectedBlockRefCount));
                OnPropertyChanged(nameof(SelectedBlockAttributes));
            }
        }

        public int SelectedBlockRefCount { get; private set; }

        public IEnumerable<AcAttributeDef> SelectedBlockAttributes
        {
            get
            {
                if (SelectedBlock == null)
                    return new List<AcAttributeDef>();
                return SelectedBlock.Attributes;
            }
        }

        public double SelectedBlockScale { get; set; }

    }
}
