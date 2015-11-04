﻿using System;

namespace disruptorpretest.Support
{
    public static class OperationExtensions
    {
        public static long Op(this Operation operation, long lhs, long rhs)
        {
            switch (operation)
            {
                case Operation.Addition:
                    return lhs + rhs;
                case Operation.Substraction:
                    return lhs - rhs;
                case Operation.And:
                    return lhs & rhs;
                default:
                    throw new ArgumentOutOfRangeException("operation");
            }
        }
    }
}