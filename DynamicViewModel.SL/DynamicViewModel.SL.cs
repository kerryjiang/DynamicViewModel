using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;

namespace DynamicViewModel
{
    public partial class DynamicViewModel
        : DynamicObject, INotifyPropertyChanged
    {
        public object this[string index]
        {
            get
            {
                object value;
                _propertyValues.TryGetValue(index, out value);
                return value;
            }

            set
            {
                Set(index, value);
                RaisePropertyChanged("[" + index + "]");
            }
        }
    }
}