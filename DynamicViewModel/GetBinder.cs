using System;
using System.Dynamic;

namespace DynamicViewModel
{
    internal sealed class GetBinder : GetMemberBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetBinder"/> class.
        /// </summary>
        /// <param name="name">The name of the member to obtain.</param>
        /// <param name="ignoreCase">Is true if the name should be matched ignoring case; false
        ///  otherwise.</param>
        public GetBinder(string name, bool ignoreCase)
            : base(name, ignoreCase) { }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic get member 
        /// operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic get member operation.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Dynamic.DynamicMetaObject"/> representing the result of the
        /// binding.
        /// </returns>
        public override DynamicMetaObject FallbackGetMember(
            DynamicMetaObject target, 
            DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}