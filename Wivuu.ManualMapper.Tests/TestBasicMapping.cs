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
        /// Tests plain-old-class-object flat mapping
        /// </summary>
        [TestMethod]
        public void TestPocoMapping()
        {
            var x = 5;

            var mapper = new Mapper();
            mapper.CreateMap<SourceType1, DestType1>()
                .ForMember(d => d.MyName, s => s.Name + x.ToString())
                .ForMember(d => d.MyValue, s => s.Value)
                // Date -> MyDate intentionally not mapped
                .Compile();

            var source = new SourceType1
            {
                Name  = "My Test Name",
                Value = 100,
                Date  = DateTime.Now
            };

            var destNew = mapper.Map<DestType1>(source);

            // Assert that mapping worked
            Assert.IsNotNull(destNew);
            Assert.AreNotEqual(source, destNew);
            Assert.AreEqual($"{source.Name}{x}", destNew.MyName);
            Assert.AreEqual(source.Value, destNew.MyValue);
            Assert.AreNotEqual(source.Date, destNew.MyDate);

            var timeChosen = DateTime.Now.AddMinutes(-5);
            var destExisting = new DestType1
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
            mapper.CreateMap<SourceType1, DestType1>()
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyValue, s => s.Value)
                // Date -> MyDate intentionally not mapped
                .Compile();

            var source = (
                from i in Enumerable.Range(0, 100)
                select new SourceType1
                {
                    Date  = DateTime.Today.AddMinutes(i),
                    Name  = $"Item {i}",
                    Value = i
                }
            ).ToList();

            var dest = source
                .ProjectTo<DestType1>(mapper)
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

            mapper.CreateMap<IEnumerable<SourceType1>, DestContainerType1>()
                .ForMember(
                    d => d.Dests, 
                    sources => Enumerable.ToList(
                        from s in sources
                        select mapper.Map<DestType1>(s))
                )
                .Compile();

            var other_source = new SourceType1 { Value = 5 };
            mapper.CreateMap<SourceType1, DestType1>()
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyValue, s => s.Value + other_source.Value)
                // Date -> MyDate intentionally not mapped
                .Compile();

            var source =
                from i in Enumerable.Range(0, 100)
                select new SourceType1
                {
                    Date  = DateTime.Today.AddMinutes(i),
                    Name  = $"Item {i}",
                    Value = i
                };

            var dest = mapper.Map<DestContainerType1>(source);

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
        /// Test construct using syntax
        /// </summary>
        [TestMethod]
        public void TestConstructUsing()
        {
            var mapper = new Mapper();
            var start = DateTime.UtcNow;
            mapper.CreateMap<SourceType1, DestType2>()
                .ConstructUsing(() => DestType2.Create(start))
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyValue, s => s.Value)
                .Compile();

            var source = new SourceType1
            {
                Date  = DateTime.MinValue,
                Name  = "Name 1",
                Value = 1
            };

            var dest = mapper.Map<DestType2>(source);

            Assert.IsNotNull(dest);
            Assert.AreNotEqual(source.Date, dest.MyDate);
            Assert.AreEqual(start, dest.MyDate);
            Assert.AreEqual(source.Name, dest.MyName);
            Assert.AreEqual(source.Value, dest.MyValue);
        }

        [TestMethod]
        public void TestExceptionForNoDestination()
        {
            var mapper = new Mapper();

            try
            {
                mapper.Map<DestType1>(new SourceType1
                {
                    Name = "Craig"
                });
                Assert.Fail($"Should have crashed with {nameof(DestinationNotMapped)}");
            }
            catch
            {
                // Pass
            }

            mapper.CreateMap<SourceType1, DestType1>()
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyValue, s => s.Value)
                // Date -> MyDate intentionally not mapped
                .Compile();

            var dest = mapper.Map<DestType1>(new SourceType1
            {
                Name = "Craig"
            });

            Assert.IsNotNull(dest);
            Assert.AreEqual("Craig", dest.MyName);
        }

        [TestMethod]
        public void TestSubObjectMapping()
        {
            var mapper = new Mapper();

            mapper.CreateMap<SourceType3, DestType3>()
                .ForMember(d => d.MyName, s => s.Name)
                .ForMember(d => d.MyChild, s => mapper.Map<DestType3_Child>(s.Child))
                .Compile();

            mapper.CreateMap<SourceType2_Child, DestType3_Child>()
                .ForMember(d => d.MyAge, s => s.Age)
                .Compile();

            var source = new SourceType3
            {
                Name = "Johnson",
                Child = new SourceType2_Child
                {
                    Age = 42
                }
            };

            var dest = mapper.Map<DestType3>(source);

            Assert.IsNotNull(dest);
            Assert.AreEqual(source.Name, dest.MyName);
            Assert.IsNotNull(dest.MyChild);
            Assert.AreEqual(source.Child.Age, dest.MyChild.MyAge);
        }
    }
}