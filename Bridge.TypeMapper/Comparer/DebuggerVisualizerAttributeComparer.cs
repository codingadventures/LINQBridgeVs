using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace LINQBridge.TypeMapper.Comparer
{
    internal class DebuggerVisualizerAttributeComparer :
        IEqualityComparer<CustomAttribute>
    {
        private const string TargetTypeProperty = "Target";
        private const string TargetTypeNameProperty = "TargetTypeName";

        public bool Equals(CustomAttribute x, CustomAttribute y)
        {
            if (!x.AttributeType.FullName.Equals(y.AttributeType.FullName)) return false;

            if (!x.Properties.Any(argument => argument.Name.Equals(TargetTypeProperty)) && !y.Properties.Any(argument => argument.Name.Equals(TargetTypeProperty)))
                if ((!x.Properties.Any(argument => argument.Name.Equals(TargetTypeNameProperty)) && !y.Properties.Any(argument => argument.Name.Equals(TargetTypeNameProperty))))
                    return false;

            var typeX =
                x.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeProperty)).Argument.Value as TypeReference;
            var typeY =
                y.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeProperty)).Argument.Value as TypeReference;

            var typeNameX =
                x.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeNameProperty)).Argument.Value;
            var typeNameY =
                y.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeNameProperty)).Argument.Value;

            if (typeX != null && typeY != null)
                return typeX.FullName == typeY.FullName;


            return typeNameX.Equals(typeNameY);

        }

        public int GetHashCode(CustomAttribute obj)
        {
            var hash = 0;

            var typeReference =
                obj.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeProperty))
                   .Argument.Value as TypeReference;


            if (typeReference == null)
            {
                var typeValue = obj.Properties.SingleOrDefault(argument => argument.Name.Equals(TargetTypeNameProperty)).Argument.Value as string;
                if (typeValue != null) hash = typeValue.GetHashCode();
            }
            else
            {
                hash = typeReference.FullName.GetHashCode();
            }



            return hash;
        }


    }
}
