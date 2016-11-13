using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Teigha.DatabaseServices;

namespace Geo7.Tools
{

    public delegate void BlockRefsAction(List<AcBlockRef> blockRefs);

    class BlockSelectPresenter : Presenter
    {
        public BlockSelectPresenter(System.Windows.Window wnd, BlockRefsAction action)
        {
            _window = wnd;
            _action = action;
            BlockNames = Ac.GetBlockNames(true, true).ToList(); // WinForms BindingSource accepts only IList or IListSource
            BlockName = BlockNames.FirstOrDefault();
        }


        private System.Windows.Window _window;
        private HashSet<ObjectId> _selectedIds = new HashSet<ObjectId>();
        private BlockRefsAction _action;


        public void AddSelectedIds(IEnumerable<ObjectId> ids)
        {
            _selectedIds.AddItems(ids);
            OnPropertyChanged(nameof(HasSelectedBlocks));
            OnPropertyChanged(nameof(SelectedBlockCount));
        }

        private void ClearSelectedIds()
        {
            _selectedIds.Clear();
            OnPropertyChanged(nameof(HasSelectedBlocks));
            OnPropertyChanged(nameof(SelectedBlockCount));

        }
        public bool HasSelectedBlocks { get { return this._selectedIds.Count > 0; } }
        public int SelectedBlockCount { get { return this._selectedIds.Count; } }

        public ICommand ClearSeclectedBlocksCommand
        {
            get { return new DelegateCommand(ClearSelectedIds, () => { return HasSelectedBlocks; }); }
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


        public List<AcBlockRef> GetSelectedBlocks()
        {
            var res = new List<AcBlockRef>();
            using (var trans = Ac.StartTransaction())
            {
                foreach (var id in _selectedIds)
                {
                    var block = new AcBlockRef(id, trans);
                    res.Add(block);
                }
            }

            return res;
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

        public List<string> BlockNames { get; private set; }

        private string _blockName;
        public string BlockName
        {
            get { return _blockName; }
            set { OnPropertyChanged(ref _blockName, value, nameof(this.BlockName)); }
        }       


        public ICommand SelectCommand
        {
            get { return new DelegateCommand(() => { _action(this.GetSelectedBlocks()); }, () => { return HasSelectedBlocks; }); }
        }
    }
}
