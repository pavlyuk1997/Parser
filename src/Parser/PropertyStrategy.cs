using Atata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public static class PropertyStrategy
    {
        public static string GetElementPropertyInStringForm<TControl, TOwner>(TOwner page, TControl element)
           where TControl : Control<TOwner>
           where TOwner : Page<TOwner>
        {
            //var type = ReflectionHelper.GetFriendlyTypeName(typeof(T));
            var type = typeof(TControl).Name;
            var index = type.IndexOf('`');
            if (index > 0)
            {
                type = type.Substring(0, index);
            }
            var propertyName = TermResolver.ToString(element.Attributes.TextContent.Value, new TermOptions { Case = TermCase.Pascal });

            return $"public {type}<_> {propertyName} {{get; private set;}}";
        }
    }
}
