using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
#endif


namespace System
{


    public class AcBlockDef : AcDbObject<BlockTableRecord>
    {
        public AcBlockDef(BlockTableRecord obj, AcTransaction trans)
            : base(obj, trans)
        {
            this.Name = obj.Name;
            this.HasAttributes = obj.HasAttributeDefinitions;

            this.IsAnonymous = obj.IsAnonymous;
            this.IsLayout = obj.IsLayout;

            var blockRefIds = obj.GetBlockReferenceIds(true, true);
            this.BlockCount = blockRefIds.Count;
            this.HasReferences = blockRefIds.Count > 0;

            InitAttributeInfos(trans);
        }
        
        public string Name { get; private set; }
        public bool IsAnonymous { get; private set; }
        public bool IsLayout { get; private set; }
        public bool HasReferences { get; private set; }
        public bool HasAttributes { get; private set; }
        public int BlockCount { get; private set; }

        public string IdAttribute { get; private set; }
        public string HeightAttribute { get; private set; }
        public string CodeAttribute { get; private set; }

        public IEnumerable<AcBlockRef> GetBlocks(AcTransaction trans)
        {
            var blockRefIds = this.GetAcObject(trans).GetBlockReferenceIds(true, true);

            foreach (ObjectId id in blockRefIds)
            {
                yield return new AcBlockRef(id, trans);
            }
        }

        public AcBlockRef FirstBlock(AcTransaction trans)
        {
            var blockRefIds = this.GetAcObject(trans).GetBlockReferenceIds(true, true);

            if (blockRefIds.Count > 0)
                return new AcBlockRef(blockRefIds[0], trans);
            else
                return null;
        }

        public AcBlockRef AddBlockRef(Point3d pos, AcTransaction trans)
        {

            //Create the block reference...
            var blockRef = new BlockReference(pos, this.ObjectId);
            blockRef.SetDatabaseDefaults(); // Default/Active layer, color, ...

            var attrRefs = new List<AttributeReference>();
            foreach (var attrDef in this.Attributes)
            {
                // this BlockDef could be initialized throught other (closed) Transaction, the same with this.Attributes
                var attDefObj = attrDef.GetAcObject(trans); // trans.GetObject<AttributeDefinition>(attrDef.ObjectId); //  attrDef.AcObject; 
                var attRef = new AttributeReference();
                attRef.SetAttributeFromBlock(attDefObj, blockRef.BlockTransform);
                attRef.TextString = attDefObj.TextString;

                blockRef.AttributeCollection.AppendAttribute(attRef);
                trans.AddNewlyCreatedDBObject(attRef, true);
                
                attrRefs.Add(attRef);
            }
            trans.AddEntity(blockRef);
            
            return new AcBlockRef(blockRef, trans, attrRefs);
        }



        private AcAttributeDefCollection attrDict = new AcAttributeDefCollection();
        public ICollection<AcAttributeDef> Attributes
        {
            get { return attrDict; }
        }

        public AcAttributeDef GetAttribute(string tag)
        {
            return attrDict.Find(tag);
        }

        private void InitAttributeInfos(AcTransaction trans)
        {
            string firstAttr = null;

            foreach (ObjectId id in this.AcObject)
            {
                var blockSubEntity = trans.GetObject<DBObject>(id);
                var blockAttrDef = blockSubEntity as AttributeDefinition;
                if ((blockAttrDef != null) && !string.IsNullOrEmpty(blockAttrDef.Tag))
                {                    
                    var attr = new AcAttributeDef(this, blockAttrDef, trans);
                    attrDict.Add(attr);
                    if (firstAttr == null)
                        firstAttr = attr.Tag;
                }
                else
                {
                    blockSubEntity.Dispose();
                }
            }

            this.IdAttribute = attrDict.FindFirstKey(Ac.IdAttributeTags);
            if (this.IdAttribute == null)
            {
                this.IdAttribute = firstAttr;
            }

            this.HeightAttribute = attrDict.FindFirstKey(Ac.HeightAttributeTags);
            this.CodeAttribute = attrDict.FindFirstKey(Ac.CodeAttributeTags);
        }



    }


    public class AcBlockRef : AcEntity<BlockReference>
    {

        public AcBlockRef(BlockReference entity, AcTransaction trans)
            : base(entity, trans)
        {
            this.Name = entity.Name;
            this.BlockDef = trans.GetBlockDef(this.Name);
            this.HasAttributes = this.BlockDef.HasAttributes;
            mPosition = entity.Position;
            mScale = entity.ScaleFactors.X;
        }


        public AcBlockRef(BlockReference entity, AcTransaction trans, IEnumerable<AttributeReference> attributes)
            : this(entity, trans)
        {
            foreach (var attr in attributes)
            {
                attrDict.Add(new AcAttributeRef(this, attr, trans));
            }
            InitAttributes();
        }

        public AcBlockRef(ObjectId id, AcTransaction trans)
            : this(trans.GetObject<BlockReference>(id), trans)
        {
            var entity = this.AcObject;   
            foreach (ObjectId attrId in entity.AttributeCollection)
            {
                var attr = trans.GetObject<AttributeReference>(attrId);
                attrDict.Add(new AcAttributeRef(this, attr, trans));
            }
            InitAttributes();
        }

        private AcAttributeRefCollection attrDict = new AcAttributeRefCollection();
        public ICollection<AcAttributeRef> Attributes
        {
            get { return attrDict; }
        }

        public AcAttributeRef GetAttribute(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return null;

            return attrDict.Find(tag.ToLower());
        }

        private void InitAttributes()
        {
            this.IdAttribute = this.GetAttribute(this.BlockDef.IdAttribute);
            this.HeightAttribute = this.GetAttribute(this.BlockDef.HeightAttribute);
            this.CodeAttribute = this.GetAttribute(this.BlockDef.CodeAttribute);
        }


        public AcBlockDef BlockDef { get; private set; }
        public string Name { get; private set; }
        public bool HasAttributes { get; private set; }

        public AcAttributeRef IdAttribute { get; private set; }
        public AcAttributeRef HeightAttribute { get; private set; }
        public AcAttributeRef CodeAttribute { get; private set; }

        private Point3d mPosition;
        public Point3d Position
        {
            get { return mPosition; }
            set
            {
                this.AcObject.Position = value;
                mPosition = value;
            }
        }

        public void ResetBlock()
        {
            this.AcObject.ResetBlock();
        }
        
        private double mScale;
        public double Scale
        {
            get { return mScale; }
            set
            {

                foreach (var attr in this.Attributes)
                {
                    attr.UpgradeOpen();
                }                

                double scaleFactor = value / this.AcObject.ScaleFactors.X;
                var scalingMtx = (Matrix3d.Scaling(scaleFactor, Position));
                this.AcObject.TransformBy(scalingMtx); // It scales block and attributes
                mScale = value;

                // After TransformBy() all attributes have IsWriteEnabled == true;
                foreach (var attr in this.Attributes)
                {
                    attr.UpgradeOpen();
                }
            }
        }            

    }
}
