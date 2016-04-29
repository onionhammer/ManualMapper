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
        public void TestBasicMapping()
        {
            var x = 5;
            Mapper.Instance.CreateMap<TestSourceType, TestDestType>()
                .ForMember(d => d.MyName, s => s.Name + x.ToString())
                .ForMember(d => d.MyValue, s => s.Value)
                // Date -> MyDate intentionally not mapped
                .Compile();

            var source = new TestSourceType
            {
                Name  = "My Test Name",
                Value = 100,
                Date  = DateTime.Now
            };

            var destNew = Mapper.Instance.Map<TestDestType>(source);

            // Assert that mapping worked
            Assert.IsNotNull(destNew);
            Assert.AreNotEqual(source, destNew);
            Assert.AreEqual($"{source.Name}{x}", destNew.MyName);
            Assert.AreEqual(source.Value, destNew.MyValue);
            Assert.AreNotEqual(source.Date, destNew.MyDate);

            var timeChosen = DateTime.Now.AddMinutes(-5);
            var destExisting = new TestDestType
            {
                MyName  = "Different Name",
                MyValue = 1,
                MyDate  = timeChosen
            };

            Mapper.Instance.Map(source, destExisting);

            // Assert that mapping worked
            Assert.IsNotNull(destExisting);
            Assert.AreNotEqual(source, destExisting);
            Assert.AreEqual($"{source.Name}{x}", destExisting.MyName);
            Assert.AreEqual(source.Value, destExisting.MyValue);
            Assert.AreNotEqual(source.Date, destExisting.MyDate);
            Assert.AreEqual(timeChosen, destExisting.MyDate);
        }
    }
}
