using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsView.Util
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [ReadOnly(true)]
    public class PGProperty : CollectionBase, ICustomTypeDescriptor
    {
        public string key;
        public bool visible;
        public object value;
        public string readonlyDisplay;
        public string category;
        public string description;

        public PGProperty(string key)
        {
            this.key = key;

            visible = true;
            value = "";
            readonlyDisplay = "[no preview]";
            description = "";
        }

        public PGProperty(string key, object value)
        {
            this.key = key;
            this.value = value;
            
            visible = true;
            readonlyDisplay = "[no preview]";
            description = "";
        }

        public PGProperty(string key, object value, string readonlyDisplay)
        {
            this.key = key;
            this.value = value;
            this.readonlyDisplay = readonlyDisplay;

            visible = true;
            description = "";
        }

        public void Add(PGProperty value)
        {
            List.Add(value);
        }

        public void Remove(string name)
        {
            foreach (PGProperty prop in List)
            {
                if (prop.key == name)
                {
                    List.Remove(prop);
                    return;
                }
            }
        }

        public override string ToString()
        {
            return readonlyDisplay;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] newProps = new PropertyDescriptor[Count];
            for (int i = 0; i < Count; i++)
            {
                PGProperty prop = (PGProperty)List[i];
                newProps[i] = new PGPropertyDescriptor(ref prop, attributes);
            }

            return new PropertyDescriptorCollection(newProps);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
    public class PGPropertyDescriptor : PropertyDescriptor
    {
        PGProperty prop;
        public PGPropertyDescriptor(ref PGProperty prop, Attribute[] attrs) : base(prop.key, attrs)
        {
            this.prop = prop;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return null;
            }
        }

        public override object GetValue(object component)
        {
            return prop.value;
        }

        public override string Description
        {
            get
            {
                return prop.description;
            }
        }

        public override string Category
        {
            get
            {
                return prop.category;
            }
        }

        public override string DisplayName
        {
            get
            {
                return prop.key;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public override void ResetValue(object component)
        {

        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            prop.value = value;
        }

        public override Type PropertyType
        {
            get { return prop.value.GetType(); }
        }
    }
}
