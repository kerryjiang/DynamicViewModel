using System;
using System.Dynamic;

namespace DynamicViewModel
{
    public sealed class DynamicViewModelFactory
    {
        public static DynamicViewModel Create()
        {
            return new DynamicViewModel();
        }

        public static DynamicViewModel Create(string json)
        {
            DynamicViewModel result;
            if (!json.TryCreateDynamic(out result))
            {
                throw new ArgumentException("parameter was not a valid JSON string");
            }

            return result;
        }

        public static DynamicViewModel Create(DynamicObject entity)
        {
            var result = new DynamicViewModel();

            foreach (var memberName in entity.GetDynamicMemberNames())
            {
                object value;
                if (entity.TryGetMember(new GetBinder(memberName, false), out value))
                {
                    result.Set(memberName, value);
                }
            }

            return result;
        }
    }
}
