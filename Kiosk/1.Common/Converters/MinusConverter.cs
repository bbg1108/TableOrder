using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Kiosk.Converters
{
    public class MinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double operand1 && double.TryParse(parameter?.ToString(), out double operand2))
            {
                if (operand1 > operand2)
                    return operand1 - operand2;
            }
            else if (value is int ioperand1 && int.TryParse(parameter?.ToString(), out int ioperand2))
            {
                if (ioperand1 > ioperand2)
                    return ioperand1 - ioperand2;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
