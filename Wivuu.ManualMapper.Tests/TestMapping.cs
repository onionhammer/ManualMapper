using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wivuu.ManualMapper.Tests
{
    public class TestSourceType
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }

    public class TestDestType
    {
        public string MyName { get; set; }
        public int MyValue { get; set; }
        public DateTime MyDate { get; set; }
    }

    [TestClass]
    public class TestMapping
    {
        [TestMethod]
        public void TestMappings()
        {
            Mapper.Instance.CreateMap<TestSourceType, TestDestType>()
                .ForMember(s => s.Name, d => d.MyName)
                .ForMember(s => s.Value, d => d.MyValue)
                //.ForMember(s => s.Date, d => d.MyDate); // Intentionally not mapped
                .Compile();

            var source = new TestSourceType
            {
                Name  = "My Test Name",
                Value = 100,
                Date  = DateTime.Now
            };

            var dest = Mapper.Instance.Map<TestDestType>(source);
        }
    }
}
