using System.Dynamic;

namespace DynamicViewModel
{
    /// <summary>
    /// Trivial override of SetMemberBinder
    /// </summary>
    internal sealed class SetBinder : SetMemberBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetBinder"/> class.
        /// </summary>
        /// <param name="name">The name of the member to obtain.</param>
        /// <param name="ignoreCase">Is true if the name should be matched ignoring case;
        ///  false otherwise.</param>
        public SetBinder(string name, bool ignoreCase)
            : base(name, ignoreCase) { }

        /// <summary>
        /// Performs the binding of the dynamic set member operation if the target dynamic object
        ///  cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic set member operation.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Dynamic.DynamicMetaObject"/> representing the result of the
        /// binding.
        /// </returns>
        public override DynamicMetaObject FallbackSetMember(
            DynamicMetaObject target,
            DynamicMetaObject value,
            DynamicMetaObject errorSuggestion)
        {
            return null;
        }
    }
}