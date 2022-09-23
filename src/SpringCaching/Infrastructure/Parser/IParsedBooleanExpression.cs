﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure.Parser
{
    public interface IParsedBooleanExpression : IParsedExpression
    {
        bool Value { get; }
    }
}