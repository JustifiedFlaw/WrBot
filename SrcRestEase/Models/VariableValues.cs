using System;
using System.Collections.Generic;

namespace SrcRestEase.Models
{
    public class VariableValues
    {
        public string Default { get; set; }

        public Dictionary<string, VariableValuesValues> Values { get; set; }
    }
}