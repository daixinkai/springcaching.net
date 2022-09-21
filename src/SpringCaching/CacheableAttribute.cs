﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using SpringCaching.Requirement;

namespace SpringCaching
{
    //[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class CacheableAttribute : CacheableBaseAttribute
    {
        public CacheableAttribute(string value) : base(value)
        {
        }
        public ExpirationPolicy ExpirationPolicy { get; set; }
        public ExpirationUnit ExpirationUnit { get; set; }
        public int ExpirationValue { get; set; }

    }
}
