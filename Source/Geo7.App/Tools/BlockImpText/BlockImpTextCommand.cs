using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;


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
using System.Diagnostics;
using Geo7.Windows;
#endif

namespace Geo7.Tools
{

    public class BlockImpTextCommand : AcCommand
    {
        protected override void ExecuteCore()
        {
            var storage = AppServices.OpenFileDialog.ShowTextLinesReadersDialog(Ac.GetLastFileName("points"));
            if (storage == null)
                return;

            using (storage)
            {
                var presenter = new BlockImpTextPresenter(storage);
                var wnd = new BlockImpTextWindow();
                wnd.DataContext = presenter;
                
                if (Ac.ShowModal(wnd))
                {
                    presenter.Import();
                }
            }
        }

        private void Wnd_Closed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }




}
