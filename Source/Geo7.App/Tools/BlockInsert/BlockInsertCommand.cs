

using System;

namespace Geo7.Tools
{
    class BlockInsertCommand : AcCommand
    {

		public BlockInsertCommand(string blockName)
		{
			this.BlockName = blockName;
		}

		private string _blockName;
		public string BlockName {
			get { return _blockName; } 
			set
			{
				_blockName = value;
				LastBlockName = value;
                using (var trans = Ac.StartTransaction(false))
                {
				    BlockScale = trans.GetSavedBlockScale(BlockName);
                }
			}
		}

		private double _blockScale = 1.0;
		public double BlockScale
		{
			get
			{
				return _blockScale;
			}
			set
			{
				if (value <= 0.0)
					_blockScale = 1.0;
				else
					_blockScale = value;
			}
		}


		private static string pointId = "1";

		private static string _lastBlockName = "";
        public static string LastBlockName
		{
			get
			{
				return _lastBlockName;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
					_lastBlockName = value;
			}
		}


		protected override void ExecuteCore()
		{
			InitBlockName();
			do
			{
				var ptRes = Ac.Editor.GetPoint("\r\n" + AppServices.Strings.EnterInsertionPoint);
				if (!ptRes.IsOK())
					return;

				var insPnt = ptRes.Value;

                var idOpt = Ac.Editor.GetPromptStringOptions("r\n" + AppServices.Strings.EnterPointId, pointId, false, true);
				var idRes = Ac.Editor.GetString(idOpt);
				if (!idRes.IsOK())
					return;

				pointId = idRes.StringResult;

                using (var trans = Ac.StartTransaction(true))
                {
                    var blockDef = trans.GetBlockDef(BlockName);
                    var blockRef = blockDef.AddBlockRef(insPnt, trans);
                    blockRef.Scale = this.BlockScale;
                    blockRef.SetIdAttribute(pointId);

                    trans.Commit();
                }

                pointId = pointId.NextPointId();

			} while (true);

		}


		private void InitBlockName()
		{
			if (!string.IsNullOrEmpty(BlockName))
				return;

			var bnOpt = Ac.Editor.GetPromptStringOptions("\r\n" + AppServices.Strings.EnterBlockName, LastBlockName, false, true);
			var bnRes = Ac.Editor.GetString(bnOpt);
            if (!bnRes.IsOK())
				return;

			var blockName = bnRes.StringResult;
			if (string.IsNullOrEmpty(blockName))
            {
				//InitBlockName();
                return;
            }

            AcDwgDb.ImportMissedBlockDef(blockName, Geo7App.Geo7Dwg);
			this.BlockName = blockName;
		}        

    }
	

}
