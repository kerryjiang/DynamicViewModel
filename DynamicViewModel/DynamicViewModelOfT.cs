using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DynamicViewModel
{
    public sealed class DynamicViewModel<TModel>
        : DynamicObject, INotifyPropertyChanged where TModel : class
    {
        /// <summary>
        /// Dictionary that holds information about the TModel public
        /// instance methods.
        /// </summary>
        /// <remarks>
        /// CA1810: Initialize reference type static fields inline.
        /// http://msdn.microsoft.com/en-us/library/ms182275(v=VS.100).aspx
        /// </remarks>
        private static readonly IDictionary<string, MethodInfo> _methodInfos
            = GetPublicInstanceMethods();

        /// <summary>
        /// Dictionary that holds information about the TModel public
        /// instance properties.
        /// </summary>
        /// <remarks>
        /// CA1810: Initialize reference type static fields inline.
        /// http://msdn.microsoft.com/en-us/library/ms182275(v=VS.100).aspx
        /// </remarks>
        private static readonly IDictionary<string, PropertyInfo> _propInfos
            = GetPublicInstanceProperties();

        private readonly TModel _model;

        /// <summary>
        /// Dictionary that holds information about the current 
        /// values of the TModel public instance properties.
        /// </summary>
        private IDictionary<string, object> _propertyValues;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="DynamicViewModel&lt;TModel&gt;"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public DynamicViewModel(TModel model)
        {
            _model = model;
            NotifyChangedProperties();
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="DynamicViewModel&lt;TModel&gt;"/> class.
        /// </summary>
        /// <param name="delegate">The @delegate.</param>
        public DynamicViewModel(Func<TModel> @delegate)
            : this(@delegate.Invoke()) { }

        /// <summary>
        /// Provides the implementation for operations that invoke a member.
        /// </summary>
        /// <param name="binder">Provides information about the dynamicoperation.
        /// </param>
        /// <param name="args">The arguments that are passed to the 
        /// object member during the invoke operation.</param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false.
        /// </returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder,
            object[] args, out object result)
        {
            result = null;

            MethodInfo methodInfo;
            if (!_methodInfos.TryGetValue(binder.Name,
                out methodInfo)) { return false; }

            methodInfo.Invoke(_model, args);
            NotifyChangedProperties();
            return true;
        }

        /// <summary>
        /// Gets the property value of the member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result of the get operation. 
        /// For example, if the method is called for a property,
        /// you can assign the property value to 
        /// <paramref name="result"/>.</param>
        /// <returns>True with the result is set.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var propertyValues = Interlocked.CompareExchange(
                ref _propertyValues, GetPropertyValues(), null);

            if (!propertyValues.TryGetValue(binder.Name,
                out result)) { return false; }

            return true;
        }

        /// <summary>
        /// Sets the property value of the member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="value">The value to set to the member. For example, 
        /// for sampleObject.SampleProperty = "Test", where sampleObject is 
        /// an instance of the class derived from the 
        /// <see cref="T:System.Dynamic.DynamicObject"/> class, 
        /// the <paramref name="value"/> is "Test".</param>
        /// <returns>True with the result is set.</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            PropertyInfo propInfo = _propInfos[binder.Name];
            propInfo.SetValue(_model, value, null);

            NotifyChangedProperties();
            return true;
        }

        /// <summary>
        /// Setting a property sometimes results in multiple properties
        /// with changed values too. For ex.: By changing the FirstName
        /// and the LastName the FullName will get updated. This method
        /// compares the m_propertyValues dictionary with the one that
        /// is obtained inside this method body. For each changed prop
        /// the PropertyChanged event is raised, notifying the callers.
        /// </summary>
        public void NotifyChangedProperties()
        {
            Interlocked.CompareExchange(
                ref _propertyValues, GetPropertyValues(), null);

            // Store the previous values in a field.
            IDictionary<String, Object> previousPropValues
                = _propertyValues;

            // Store the  current values in a field.
            IDictionary<String, Object> currentPropValues
                = GetPropertyValues();

            // Since we will be raising the PropertyChanged event
            // we want the caller to bind in the current values
            // and not the previous.
            _propertyValues
                = currentPropValues;

            foreach (KeyValuePair<string, object> propValue
                in currentPropValues.Except(previousPropValues))
            {
                RaisePropertyChanged(propValue.Key);
            }
        }

        /// <summary>
        /// Gets the public instance methods of the TModel type.
        /// </summary>
        /// <returns>
        /// A dictionary that holds information about TModel public
        /// instance properties.
        /// </returns>
        private static IDictionary<string, MethodInfo> GetPublicInstanceMethods()
        {
            var methodInfoDictionary = new Dictionary<string, MethodInfo>();
            MethodInfo[] methodInfos = typeof(TModel).GetMethods(
                BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo methodInfo in methodInfos)
            {
                if (methodInfo.Name.StartsWith("get_") ||
                    methodInfo.Name.StartsWith("set_")) { continue; }
                methodInfoDictionary.Add(methodInfo.Name, methodInfo);
            }

            return methodInfoDictionary;
        }

        /// <summary>
        /// Gets the public instance properties of the TModel type.
        /// </summary>
        /// <returns>
        /// A dictionary that holds information about TModel public
        /// instance properties.
        /// </returns>
        private static IDictionary<string, PropertyInfo> GetPublicInstanceProperties()
        {
            var propInfoDictionary = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] propInfos = typeof(TModel).GetProperties(
                BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propInfo in propInfos)
            {
                propInfoDictionary.Add(propInfo.Name, propInfo);
            }

            return propInfoDictionary;
        }

        /// <summary>
        /// Gets the property values about the TModel public instance properties.
        /// </summary>
        /// <returns>A dictionary that holds information about the current 
        /// values of the TModel public instance properties.</returns>
        private IDictionary<string, object> GetPropertyValues()
        {
            var bindingPaths = new Dictionary<string, object>();
            PropertyInfo[] propInfos = typeof(TModel).GetProperties(
                BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propInfo in propInfos)
            {
                bindingPaths.Add(
                    propInfo.Name,
                    propInfo.GetValue(_model, null));
            }

            return bindingPaths;
        }

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" />
        ///   event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> 
        /// instance containing the event data.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler temp =
                Interlocked.CompareExchange(ref PropertyChanged, null, null);

            if (temp != null)
            {
                temp(this, e);
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
