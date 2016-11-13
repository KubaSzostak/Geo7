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
    

    public class AcAttributeDef : AcText<AttributeDefinition>
    {
        public AcAttributeDef(AttributeDefinition entity, AcTransaction trans)
            : base(entity, trans)
        {
            mTag = entity.Tag;
            mInvisible = entity.Invisible;
            this.AttrVisiblePresenter = !mInvisible;

        }

        private string mTag;
        public string Tag
        {
            get { return mTag; }
            set
            {
                this.AcObject.Tag = value;
                mTag = value;
            }
        }

        private bool mInvisible;
        public bool Invisible
        {
            get { return mInvisible; }
            set
            {
                this.AcObject.Invisible = value;
                mInvisible = value;
                this.AttrVisiblePresenter = !value;
            }
        }

        // For editing only
        public bool AttrVisiblePresenter { get; set; }
    }
    


    public class AcAttributeRef : AcText<AttributeReference>
    {
        public AcAttributeRef(AttributeReference entity, AcTransaction trans)
            : base(entity, trans)
        {
            mTag = entity.Tag;
            mInvisible = entity.Invisible;
        }
        private string mTag;
        public string Tag
        {
            get { return mTag; }
            set {
                this.AcObject.Tag = value;
                mTag = value;
            }
        }

        private bool mInvisible;
        public bool Invisible
        {
            get { return mInvisible; }
            set
            {
                this.AcObject.Invisible = value;
                mInvisible = value;
            }
        }

    }


    public abstract class AcKeyedCollection<TVal> : System.Collections.ObjectModel.KeyedCollection<string, TVal> 
    {
        public TVal Find(string key)
        {
            var res = default(TVal);

            if (this.Dictionary == null)
                return res;

            if (string.IsNullOrEmpty(key as string))
                return res;

            key = key.ToLowerInvariant();
            if (this.Dictionary.TryGetValue(key, out res))
                return res;
            else
                return default(TVal);
        }

        public string FindFirstKey(params string[] keys)
        {
            foreach (var item in this)
            {
                foreach (var key in keys)
                {
                    var itemKey = this.GetKeyForItem(item);
                    
                    if (string.Equals(key, itemKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return itemKey;
                    }
                }
            }
            return null;
        }
    }

    public class AcAttributeDefCollection : AcKeyedCollection<AcAttributeDef>
    {
        protected override string GetKeyForItem(AcAttributeDef item)
        {
            return item.Tag.ToLowerInvariant();
        }
    }

    public class AcAttributeRefCollection : AcKeyedCollection<AcAttributeRef>
    {
        protected override string GetKeyForItem(AcAttributeRef item)
        {
            return item.Tag.ToLowerInvariant();
        }
    }

}
