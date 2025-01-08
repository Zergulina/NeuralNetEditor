using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NeuralNetEditor.Helpers
{
    internal static class ConverterToStringExtention
    {
        public static string ToArrayString(this uint[] arr)
        {
            var result = "(";
            for (var i = 0; i < arr.Length - 1; i++)
            {
                result += arr[i] + ", ";
            }
            result += arr[arr.Length - 1];
            return result + ")";
        }
    }
}
