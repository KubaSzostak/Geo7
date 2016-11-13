
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
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Geo7.Tools
{
	class BlockInsertCommand : AcCommand
    {

		public BlockInsertCommand(string blockName)
		{
			this.BlockName = blockName;
		}

		private string mBlockName;
		public string BlockName {
			get { return mBlockName; } 
			set
			{
				mBlockName = value;
				LastBlockName = value;
                using (var trans = Ac.StartTransaction())
                {
				    BlockScale = trans.GetSavedBlockScale(BlockName);
                }
			}
		}

		private double mBlockScale = 1.0;
		public double BlockScale
		{
			get
			{
				return mBlockScale;
			}
			set
			{
				if (value <= 0.0)
					mBlockScale = 1.0;
				else
					mBlockScale = value;
			}
		}


		private static string pointId = "1";

		private static string mLastBlockName = "";
        public static string LastBlockName
		{
			get
			{
				return mLastBlockName;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
					mLastBlockName = value;
			}
		}


		protected override void ExecuteCore()
		{
			InitBlockName();
			do
			{
				PromptPointResult ptRes = Ac.Editor.GetPoint("\r\n" + AcConsts.EnterInsertionPoint);
				if (ptRes.Status != PromptStatus.OK)
					return;
				Point3d insPnt = ptRes.Value;

				PromptStringOptions idOpt = new PromptStringOptions("\r\n" + AcConsts.EnterPointId);
				idOpt.DefaultValue = pointId;
				idOpt.UseDefaultValue = true;
				idOpt.AllowSpaces = false;

				var idRes = Ac.Editor.GetString(idOpt);
				if (idRes.Status != PromptStatus.OK)
					return;
				pointId = idRes.StringResult;

				InsertBlock(insPnt, pointId);
				pointId = pointId.NextPointId();

			} while (true);

		}


		private void InitBlockName()
		{
			if (!string.IsNullOrEmpty(BlockName))
				return;

			PromptStringOptions bnOpt = new PromptStringOptions("\r\n" + AcConsts.EnterBlockName );
			bnOpt.DefaultValue = LastBlockName;
			bnOpt.UseDefaultValue = true;
			bnOpt.AllowSpaces = false;

			var bnRes = Ac.Editor.GetString(bnOpt);
			if (bnRes.Status != PromptStatus.OK)
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


		private void InsertBlock(Point3d insPnt, string pointId)
        {
            using (var docLock = Ac.Doc.LockDocument())
            {
                using (var trans = Ac.StartTransaction())
                {
                    var blockDef = trans.GetBlockDef(BlockName);
                    var blockRef = blockDef.AddBlockRef(insPnt, trans);
                    blockRef.Scale = this.BlockScale;
                    if (blockRef.IdAttribute != null)
                        blockRef.IdAttribute.TextString = pointId;

                    trans.Commit();
                }
            }
		}
        

    }
	

}
