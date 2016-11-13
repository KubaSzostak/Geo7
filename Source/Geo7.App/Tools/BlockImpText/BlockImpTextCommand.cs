using Geo7.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;



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
