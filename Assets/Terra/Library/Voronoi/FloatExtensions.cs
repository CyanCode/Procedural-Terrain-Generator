// Copyright (C) afuzzyllama. All rights reserved
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace PixelsForGlory.Extensions
{
    public static class FloatExtensions
    {
        public static float SmallNumber = 0.00000001f;
        public static float SmallishNumber = 0.0001f;
        public static float NotReallySmallishNumber = 0.001f;

        public static bool IsAlmostZero(this float number, float tolerance = /* SmallNumber */ 0.00000001f)
        {
            return Math.Abs(number) <= tolerance;
        }

        public static bool IsAlmostEqualTo(this float numberA, float numberB, float tolerance = /* SmallNumber */ 0.00000001f)
        {
            return Math.Abs(numberA - numberB) <= tolerance;
        }
    }
}
