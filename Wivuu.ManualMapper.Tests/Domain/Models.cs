using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Wivuu.ManualMapper.Tests.Domain
{
    [DebuggerDisplay("{Value} - {Name} - {Date}")]
    public class TestSourceType
    {
        [Key]
        public int Value { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

    [DebuggerDisplay("{MyValue} - {MyName}")]
    public class TestDestType
    {
        public string MyName { get; set; }
        public int MyValue { get; set; }
        public DateTime MyDate { get; set; }
    }

    public class TestContainerType
    {
        public List<TestDestType> Dests { get; set; }
    }
}
