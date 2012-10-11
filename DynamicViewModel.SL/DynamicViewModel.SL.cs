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
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(string.Format("[{0}]", propertyName)));
        }
    }
}