﻿using SpringCaching.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SpringCaching
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class CacheBaseAttribute : Attribute
    {
        public CacheBaseAttribute(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        /// <summary>
        /// Names of the caches in which method invocation results are stored.
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression for computing the key dynamically.
        /// Default is "", meaning all method parameters are considered as a key, unless a custom keyGenerator has been configured.
        /// </summary>
        public string? Key { get; set; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression used for making the method caching conditional.
        /// Default is "", meaning the method result is always cached.
        /// </summary>
        public string? Condition { get; set; }
        /// <summary>
        /// The bean name of the custom <see cref="IKeyGenerator"/> to use.
        /// Mutually exclusive with the key attribute.
        /// </summary>
        public Type? KeyGenerator { get; set; }
        /// <summary>
        /// The bean name of the custom <see cref="IPredicateGenerator"/> to use.
        /// Mutually exclusive with the key attribute.
        /// </summary>
        public Type? ConditionGenerator { get; set; }

    }
}
