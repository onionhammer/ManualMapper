using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wivuu.ManualMapper.Tests.Domain
{
    public class TestSourceType
    {
        [Key]
        public int Value { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

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
