﻿namespace GeekLearning.Bouncer.EntityFrameworkCore.Caching
{
    using System;

    public interface ICacheableObject
    {
        string CacheKey { get; }

        DateTime CacheValuesDateTime { get; set; }
    }
}