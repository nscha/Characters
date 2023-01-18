﻿namespace Kenedia.Modules.Characters
{
    using System;
    using System.Collections.Generic;

    internal static class DisposableExtensions
    {
        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (var d in disposables)
            {
                d?.Dispose();
            }
        }
    }
}