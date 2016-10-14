using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Wivuu.ManualMapper.Tests.Domain
{
    [DebuggerDisplay("{Value} - {Name} - {Date}")]
    public class SourceType1
    {
        [Key]
        public int Value { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

    [DebuggerDisplay("{MyValue} - {MyName}")]
    public class DestType1
    {
        public string MyName { get; set; }
        public int MyValue { get; set; }
        public DateTime MyDate { get; set; }
    }

    public class DestContainerType1
    {
        public List<DestType1> Dests { get; set; }
    }

    [DebuggerDisplay("{MyValue} - {MyName}")]
    public class DestType2
    {
        public string MyName { get; set; }
        public int MyValue { get; set; }
        public DateTime MyDate { get; set; }

        private DestType2()
        {
        }

        public static DestType2 Create(DateTime start) =>
            new DestType2 { MyDate = start };
    }

    public class SourceType3
    {
        public string Name { get; set; }

        public SourceType2_Child Child { get; set; }
    }

    public class SourceType2_Child
    {
        public int Age { get; set; }
    }

    public class DestType3
    {
        public string MyName { get; set; }

        public DestType3_Child MyChild { get; set; }
    }

    public class DestType3_Child
    {
        public int MyAge { get; set; }
    }
}
