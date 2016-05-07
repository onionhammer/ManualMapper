using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wivuu.ManualMapper.Tests.Base;
using Wivuu.ManualMapper.Tests.Domain;

namespace Wivuu.ManualMapper.Tests
{
    [TestClass]
    public class TestDatabaseMapping : DatabaseTests
    {
        [TestMethod]
        public async Task TestQueryableMapping()
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

            // Insert items
            Db.MySources.AddRange(source);
            await Db.SaveChangesAsync();

            // Retrieve dest
            var dest = await Db.MySources
                .ProjectTo<TestDestType>(mapper)
                .ToListAsync();

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