// The class belongs to the extensions namespace
namespace Oi.Extensions
{
    /// <summary>
    /// Extension methods to the double class.
    /// </summary>
    public static class Double_Ext
    {
        /// <summary>
        /// Convert an angle from radians to degrees.
        /// </summary>
        /// <param name="radians">The value to convert.</param>
        /// <returns>A double.</returns>
        public static double Ext_ToDegrees(this double radians)
        {
            return radians * ((double)180 / Math.PI);
        }

        /// <summary>
        /// Convert an angle from degrees to radians.
        /// </summary>
        /// <param name="degrees">The value to convert.</param>
        /// <returns>A double.</returns>
        public static double Ext_ToRadians(this double degrees)
        {
            return degrees * (Math.PI / (double)180);
        }

        /// <summary>
        /// Converts a value to project units from internal units.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="unitType">The UnitType to convert using.</param>
        /// <returns>The converted value.</returns>
        public static double Ext_InternalToProject(this double value, ForgeTypeId unitType)
        {
            if (unitType != null && UnitUtils.IsUnit(unitType))
            {
                return UnitUtils.ConvertFromInternalUnits(value, unitType);
            }

            return value;
        }

        /// <summary>
        /// Converts a value from project units to internal units.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="unitType">The UnitType to convert using.</param>
        /// <returns>The converted value.</returns>
        public static double Ext_ProjectToInternal(this double value, ForgeTypeId unitType)
        {
            if (unitType != null && UnitUtils.IsUnit(unitType))
            {
                return UnitUtils.ConvertToInternalUnits(value, unitType);
            }

            return value;
        }
    }
}
