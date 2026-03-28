using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PCAN
{
    // int と 16進数文字列 を相互変換するコンバーター
    public class HexToIntConverter : IValueConverter
    {
        // Model(int)からView(string)へ渡すときの処理（表示）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return $"0x{intValue:X2}"; // 2桁の16進数（大文字）で表示
            }
            return "0x00";
        }

        // View(string)からModel(int)へ戻すときの処理（入力）
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is string strValue)
            {
                strValue = strValue.Replace("0x", "").Replace("0X", "").Trim();
                if (string.IsNullOrEmpty(strValue)) return 0;
                try
                {
                    return System.Convert.ToInt32(strValue, 16);
                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }
    }
}
