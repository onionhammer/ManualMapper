using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wivuu.ManualMapper.Tests.Domain;

namespace Wivuu.ManualMapper.Tests
{
    [TestClass]
    public class TestBasicMapping
    {
        /// <summary>
        /// Tests plain-old-class-object mapping
        /// </summary>
        [TestMethod]
        public void TestPocoMapping()
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

        /// <summary>
        /// Tests mapping with projections with enumerables
        /// </summary>
        [TestMethod]
        public void TestEnumerableProjection()
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

        /// <summary>
        /// Tests mapping ienumerable to non-enumerable with sub-mapping
        /// </summary>
        [TestMethod]
        public void TestSubMapping()
        {
            var mapper = new Mapper();

            mapper.CreateMap<IEnumerable<TestSourceType>, TestContainerType>()
                .ForMember(
                    d => d.Dests, 
                    sources => Enumerable.ToList(
                        from s in sources
                        select mapper.Map<TestDestType>(s))
                )
                .Compile();

            var other_source = new TestSourceType { Value = 5 };
            mapper.CreateMap<TestSourceType, TestDestType>()
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyValue, s => s.Value + other_source.Value)
                // Date -> MyDate intentionally not mapped
                .Compile();

            var source =
                from i in Enumerable.Range(0, 100)
                select new TestSourceType
                {
                    Date  = DateTime.Today.AddMinutes(i),
                    Name  = $"Item {i}",
                    Value = i
                };

            var dest = mapper.Map<TestContainerType>(source);

            Assert.AreEqual(source.Count(), dest.Dests.Count);
            Enumerable
                .Zip(source, dest.Dests, (x, y) => Tuple.Create(x, y))
                .All(t =>
                {
                    Assert.AreEqual(t.Item1.Name, t.Item2.MyName);
                    Assert.AreEqual(t.Item1.Value + other_source.Value, t.Item2.MyValue);
                    Assert.AreNotEqual(t.Item1.Date, t.Item2.MyDate);

                    return true;
                });
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestConstructUsing()
        {
            var mapper = new Mapper();
            var start = DateTime.UtcNow;
            mapper.CreateMap<TestSourceType, TestDestType>()
                .ConstructUsing(() => new TestDestType
                {
                    MyDate = start
                })
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyValue, s => s.Value)
                .Compile();

            var source = new TestSourceType
            {
                Date  = DateTime.MinValue,
                Name  = "Name 1",
                Value = 1
            };

            var dest = mapper.Map<TestDestType>(source);

            Assert.IsNotNull(dest);
            Assert.AreNotEqual(source.Date, dest.MyDate);
            Assert.AreEqual(start, dest.MyDate);
            Assert.AreEqual(source.Name, dest.MyName);
            Assert.AreEqual(source.Value, dest.MyValue);
        }
    }
}