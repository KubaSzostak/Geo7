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
        public AcAttributeDef(AcBlockDef owner, AttributeDefinition entity, AcTransaction trans)
            : base(entity, trans)
        {
            _tag = entity.Tag;
            _invisible = entity.Invisible;
            this.AttrVisiblePresenter = !_invisible;
            Owner = owner;
        }

        public AcBlockDef Owner { get; private set; }

        private string _tag;
        public string Tag
        {
            get { return _tag; }
            set
            {
                this.AcObject.Tag = value;
                _tag = value;
            }
        }

        private bool _invisible;
        public bool Invisible
        {
            get { return _invisible; }
            set
            {
                this.AcObject.Invisible = value;
                _invisible = value;
                this.AttrVisiblePresenter = !value;
            }
        }

        /// <summary>
        /// Use it for binding to controls only
        /// </summary>
        public bool AttrVisiblePresenter { get; set; }

        public override string ToString()
        {
            return Owner.Name + "." + Tag;
        }
    }
    


    public class AcAttributeRef : AcText<AttributeReference>
    {
        public AcAttributeRef(AcBlockRef owner, AttributeReference entity, AcTransaction trans)
            : base(entity, trans)
        {
            _tag = entity.Tag;
            _invisible = entity.Invisible;
            Owner = owner;
        }

        public AcBlockRef Owner { get; private set; }

        private string _tag;
        public string Tag
        {
            get { return _tag; }
            set {
                this.AcObject.Tag = value;
                _tag = value;
            }
        }

        private bool _invisible;
        public bool Invisible
        {
            get { return _invisible; }
            set
            {
                this.AcObject.Invisible = value;
                _invisible = value;
            }
        }

        public override string ToString()
        {
            return Owner.Name + "." + Tag;
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
