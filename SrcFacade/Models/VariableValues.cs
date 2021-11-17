using System;
using System.Collections.Generic;

namespace SrcFacade.Models
{
    public class VariableValues
    {
        public string Default { get; set; }

        public Dictionary<string, VariableValuesValues> Values { get; set; }
    }
}