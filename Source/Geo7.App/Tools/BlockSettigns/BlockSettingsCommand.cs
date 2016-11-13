using Geo7.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geo7.Tools
{

    public class BlockSettingsCommand : AcCommand
    {
        protected override void ExecuteCore()
        {
            var presenter = new BlocksSettingsPresenter();
            if (presenter.BlockList.Count < 1)
                throw new System.Exception(AppServices.Strings.BlocksNotFond);

            var wnd = new BlockSettingsWindow();
            wnd.DataContext = presenter;
            Ac.ShowModal(wnd);
        }

    }

}
