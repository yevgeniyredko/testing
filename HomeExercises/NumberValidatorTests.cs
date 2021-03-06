﻿using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [TestCase(4, 2, true, null, ExpectedResult = false)]
        [TestCase(4, 2, true, "", ExpectedResult = false)]
        [TestCase(4, 2, true, "0.", ExpectedResult = false)]
        [TestCase(4, 2, true, ".0", ExpectedResult = false)]
        [TestCase(4, 2, true, "asd", ExpectedResult = false)]
        [TestCase(4, 2, true, "a.sd", ExpectedResult = false)]
        [TestCase(4, 2, true, "00000", ExpectedResult = false)]
        [TestCase(4, 2, true, "000.00", ExpectedResult = false)]
        [TestCase(4, 2, true, "0.000", ExpectedResult = false)]
        [TestCase(4, 2, false, "-0000", ExpectedResult = false)]
        [TestCase(4, 2, true, "+0000", ExpectedResult = false)]
        [TestCase(4, 2, true, "-1.2", ExpectedResult = false)]
        [TestCase(4, 2, false, "-1.2", ExpectedResult = true)]
        [TestCase(4, 2, true, "+1.2", ExpectedResult = true)]
        [TestCase(4, 2, true, "1234", ExpectedResult = true)]
        [TestCase(4, 2, true, "12.34", ExpectedResult = true)]
        public static bool IsValidNumber(int precision, int scale, bool onlyPositive, string value)
        {
            return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
        }
        
        [TestCase(-1, 2, true, TestName = "When precision is negative")]
        [TestCase(0, 2, true, TestName = "When precision is zero")]
        [TestCase(1, -2, true, TestName = "When scale is negative")]
        [TestCase(1, 2, false, TestName = "When scale is greater than precision")]
        public static void Constructing_ShouldThrow(int precision, int scale, bool onlyPositive)
        {
            Assert.That(() => new NumberValidator(precision, scale, onlyPositive), Throws.ArgumentException);
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("scale must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}