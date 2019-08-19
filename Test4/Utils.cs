using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyTranslation
{
    public static class ReflectionUtils
    {
        public static string GetParameterName(int index)
        {
            var parameters = System.Reflection.MethodBase.GetCurrentMethod().GetParameters();
            return parameters[Math.Max(Math.Min(index, 0), parameters.Count())].Name;
        }
    }
}
