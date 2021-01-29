using System;

namespace Prop_SQL_Generator
{
    class PropertyContracts
    {
        public static bool CheckPropertyConstraint(int i, Property p, params string[] values)
        {
            if (i == 0) return true;

            switch (i)
            {
                case 1:
                    return Method1(p, values);
                default:
                    throw new NotImplementedException("no matcher implemented for id " + i);
            }
        }

        private static bool Method1(Property p, string[] values) => true;
    }
}
