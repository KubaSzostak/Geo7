

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Geo7.Windows;

namespace Geo7.Tools
{

    public class BlockExpTextCommand : AcCommand
    {
        protected override void ExecuteCore()
        {
            if (Ac.GetBlockNames(true, true).Count< 1)
                throw new System.Exception(AppServices.Strings.BlocksNotFond);

            var selWnd = new BlockSelectWindow(OnBlocksSelected);
            Ac.ShowModal(selWnd);  
        }

        private void OnBlocksSelected(List<AcBlockRef> blockRefs)
        {
            var expWnd = new BlockExpTextWindow(blockRefs);
            Ac.ShowModal(expWnd);
        }

    }

}
