using System;
using System.Linq;
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

            var mapper = new Mapper();
            mapper.CreateMap<TestSourceType, TestDestType>()
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

            var destNew = mapper.Map<TestDestType>(source);

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

            mapper.Map(source, destExisting);

            // Assert that mapping worked
            Assert.IsNotNull(destExisting);
            Assert.AreNotEqual(source, destExisting);
            Assert.AreEqual($"{source.Name}{x}", destExisting.MyName);
            Assert.AreEqual(source.Value, destExisting.MyValue);
            Assert.AreNotEqual(source.Date, destExisting.MyDate);
            Assert.AreEqual(timeChosen, destExisting.MyDate);
        }

        [TestMethod]
        public void TestEnumerableMapping()
        {
            var mapper = new Mapper();
            mapper.CreateMap<TestSourceType, TestDestType>()
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyValue, s => s.Value)
                // Date -> MyDate intentionally not mapped
                .Compile();

            var source = (
                from i in Enumerable.Range(0, 100)
                select new TestSourceType
                {
                    Date  = DateTime.Today.AddMinutes(i),
                    Name  = $"Item {i}",
                    Value = i
                }
            ).ToList();

            var dest = source
                .ProjectTo<TestDestType>(mapper)
                .ToList();

            Assert.AreEqual(source.Count, dest.Count);
            Enumerable
                .Zip(source, dest, (x, y) => Tuple.Create(x, y))
                .All(t =>
                {
                    Assert.AreEqual(t.Item1.Name, t.Item2.MyName);
                    Assert.AreEqual(t.Item1.Value, t.Item2.MyValue);
                    Assert.AreNotEqual(t.Item1.Date, t.Item2.MyDate);

                    return true;
                });
        }

        [TestMethod]
        public void TestQueryableMapping()
        {
            var mapper = new Mapper();
            mapper.CreateMap<TestSourceType, TestDestType>()
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyValue, s => s.Value)
                // Date -> MyDate intentionally not mapped
                .Compile();

            var source = (
                from i in Enumerable.Range(0, 100)
                select new TestSourceType
                {
                    Date = DateTime.Today.AddMinutes(i),
                    Name = $"Item {i}",
                    Value = i
                }
            ).ToList();

            var dest = source
                .AsQueryable()
                .ProjectTo<TestDestType>(mapper)
                .ToList();

            Assert.AreEqual(source.Count, dest.Count);
            Enumerable
                .Zip(source, dest, (x, y) => Tuple.Create(x, y))
                .All(t =>
                {
                    Assert.AreEqual(t.Item1.Name, t.Item2.MyName);
                    Assert.AreEqual(t.Item1.Value, t.Item2.MyValue);
                    Assert.AreNotEqual(t.Item1.Date, t.Item2.MyDate);

                    return true;
                });
        }
    }
}
