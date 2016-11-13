using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;


#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Colors;
using Bricscad.ApplicationServices;
using Bricscad.Windows;
using Bricscad.EditorInput;

using AcApp = Bricscad.ApplicationServices.Application;
using Geo7.Tools;
#endif

namespace Geo7.Tools
{
    public class BlockFindPresenter : Presenter
    {
        public BlockFindPresenter()
        {
            FindCommand = new AcDelegateCommand(Find, CanFind);
            FindNextCommand = new AcDelegateCommand(FindNext, CanFind);
            FoundInfoVisibility = System.Windows.Visibility.Collapsed;
        }

        public string FindText { get; set; }
        public ICommand FindCommand { get; set; }
        public ICommand FindNextCommand { get; set; }


        private List<FindTextInfo> Find(string text)
        {
            var findList = new List<FindTextInfo>();
            if (string.IsNullOrEmpty(text))
            {
                return findList;
            }
            var findTextLower = text.Trim().ToLower();

            using (var trans = Ac.StartTransaction())
            {
                var blockRefs = trans.GetAllEntities<BlockReference>().Where(b => b.AttributeCollection.Count > 0);
                foreach (var block in blockRefs)
                {
                    if (!trans.IsDisplayed(block))
                        continue;
                    for (int iAttr = 0; iAttr < block.AttributeCollection.Count; iAttr++)
                    {
                        var attrId = block.AttributeCollection[iAttr];
                        var attr = trans.GetObject<AttributeReference>(attrId);
                        if (!attr.Visible || attr.Invisible)
                            continue;
                        if (attr.TextString.ToLower().Contains(findTextLower))
                        {
                            var fti = new FindTextInfo();
                            fti.Text = attr.TextString;
                            fti.Position = block.Position;
                            fti.Source = block.Name + "." + attr.Tag;
                            findList.Add(fti);
                        }
                    }
                }
            }
            return findList;
        }

        private void Find()
        {
            var findList = Find(FindText);
            FindList = findList;

            if (findList.Count > 0)
            {
                var selItem = findList.FirstOrDefault(i => i.Text == FindText); // Najpierw spróbuj znaleźć dokładny tekst
                if (selItem == null)
                    selItem = findList.FirstOrDefault(i => i.Text.ToLower() == FindText.ToLower()); // Potem szukaj Case Insensitive
                if (selItem == null)
                    selItem  = findList.First(); // Jeżeli ciągle nic nie znajdzie - ustaw pierwszy z brzegu

                SelectedItem = selItem;
                SetFoundInfo(null);
            }
            else
            {
                SetFoundInfo(string.Format(AppServices.Strings.XNotFound, FindText));
            }

            this.OnPropertyChanged(nameof(FindList));
        }

        private void FindNext()
        {
            if (FindText == null)
            {
                return;
            }

            int findNo;
            try
            {
                findNo = FindText.ToInt();
            }
            catch (System.Exception ex)
            {
                SetFoundInfo(ex.Message);
                return;
            }
            FindText = (++findNo).ToString();
            this.OnPropertyChanged(nameof(FindText));
            Find();
        }

        private bool CanFind()
        {
            return Ac.Db != null;
            //return !string.IsNullOrEmpty(FindText);
        }

        public System.Windows.Visibility FoundInfoVisibility { get; set; }
        public string FoundInfo { get; set; }

        private void SetFoundInfo(string info)
        {
            if (string.IsNullOrEmpty(info))
            {
                FoundInfoVisibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                FoundInfo = info;
                FoundInfoVisibility = System.Windows.Visibility.Visible;
            }
            OnPropertyChanged(nameof(FoundInfoVisibility));
            OnPropertyChanged(nameof(FoundInfo));
        }

        public IEnumerable<FindTextInfo> FindList { get; set; }

        private FindTextInfo mSelectedItem;
        public FindTextInfo SelectedItem
        {
            get
            {
                return mSelectedItem;
            }
            set
            {
                mSelectedItem = value;
                if (mSelectedItem != null)
                {
                    Ac.ZoomTo(mSelectedItem.Position);
                }
            }
        }

        public class FindTextInfo
        {
            public string Text { get; set; }
            public Point3d Position { get; set; }
            public string Source { get; set; }
        }

    }
}
