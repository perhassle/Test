using System.Collections.Generic;
using DbUp.Engine;

namespace DbUpWrapper.SqlWriters
{
    internal interface ISqlWriter
    {
        int Write(List<SqlScript> scripts);
    }
}