using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace DynamicViewModel
{
    public partial class DynamicViewModel 
        : DynamicObject, INotifyPropertyChanged
    {
        private readonly IDictionary<string, object> _propertyValues = new Dictionary<string, object>();

        /// <summary>
        /// When overriden, used to obtain a reference to this as a dynamic object.
        /// </summary>
        protected dynamic This { get { return this; } }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from
        ///  the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to 
        /// specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic 
        /// operation. The binder.Name property provides the name of the member on which the 
        /// dynamic operation is performed. For example, for the 
        /// Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an 
        /// instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> 
        /// class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies 
        /// whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is 
        /// called for a property, you can assign the property value to <paramref name="result"/>.
        /// </param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, 
        /// the run-time binder of the language determines the behavior. 
        /// (In most cases, a run-time exception is thrown.)
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            if (!_propertyValues.ContainsKey(binder.Name))
                return false;

            result = _propertyValues[binder.Name];
            return true;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from
        ///  the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to 
        /// specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic 
        /// operation. The binder.Name property provides the name of the member to which the value 
        /// is being assigned. For example, for the statement sampleObject.SampleProperty = "Test",
        ///  where sampleObject is an instance of the class derived from the
        ///  <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns
        ///  "SampleProperty". The binder.IgnoreCase property specifies whether the member name is
        ///  case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for 
        /// sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class 
        /// derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the 
        /// <paramref name="value"/> is "Test".</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false,
        ///  the run-time binder of the language determines the behavior. 
        /// (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // Check to see if the property exists, and has changed).
            if (_propertyValues.ContainsKey(binder.Name))
            {
                // Check to see if the property has changed.
                if (_propertyValues[binder.Name].Equals(value))
                {
                    return true;
                }
            }

            _propertyValues[binder.Name] = value;

            RaisePropertyChanged(binder.Name);
            return true;
        }

        public void Set(string key, object value)
        {
            var propertyInfo = GetType().GetProperty(key);
            if (propertyInfo != null)
            {
                var stringValue = value == null ? null : value.ToString();
                var propertyValue = Convert.ChangeType(stringValue, propertyInfo.PropertyType, null);
                propertyInfo.SetValue(this, propertyValue, null);
            }
            else
            {
                // Tries to set the property named 'key' with value of 'value'
                //  - parameter 'false' indicates key is case-sensitive.
                TrySetMember(new SetBinder(key, false), value);
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the <see cref="System.Object" /> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
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
                RaisePropertyChanged(index);
            }
        }


        /// <summary>
        /// Updates the properties of this viewModel according the given entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void UpdateProperties(DynamicViewModel entity)
        {
            foreach (var memberName in entity._propertyValues.Keys)
            {
                object value;

                if (entity._propertyValues.TryGetValue(memberName, out value))
                {
                    if (value is DynamicViewModel)
                    {
                        var childViewModel = value as DynamicViewModel;
                        var oldValue = this[memberName] as DynamicViewModel;

                        if (oldValue != null)
                        {
                            oldValue.UpdateProperties(childViewModel);
                            continue;
                        }
                    }

                    this[memberName] = value;
                }
            }
        }
    }
}