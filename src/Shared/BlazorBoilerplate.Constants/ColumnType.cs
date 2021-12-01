using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Constants
{
    public enum ColumnType
    {
        Unknown = 0,
        String = 1,
        Int = 2,
        Float = 3,
        Category = 4,
        Boolean = 5,
        Datetime = 6,
        Ignore = 7
    }
}
