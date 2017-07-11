using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Shouldly;
using NUnit.Framework;

namespace AutoMapperSamples.Mappers
{
    namespace Arrays
    {
        [TestFixture]
        public class MappingElementTypes
        {
            public class Source
            {
                public int Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
            }

            [Test]
            public void Example()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Source, Dest>();
                });

                var sourceArray = new[]
                {
                    new Source { Value = 5 },
                    new Source { Value = 10 },
                    new Source { Value = 15 }
                };

                var destArray = config.CreateMapper().Map<Source[], Dest[]>(sourceArray);

                destArray.Length.ShouldBe(3);
                destArray[0].Value.ShouldBe(5);
                destArray[1].Value.ShouldBe(10);
                destArray[2].Value.ShouldBe(15);
            }
        }

        [TestFixture]
        public class MappingArrayMemberTypes
        {
            public class Order
            {
                private IList<OrderLine> _lineItems = new List<OrderLine>();

                public OrderLine[] LineItems { get { return _lineItems.ToArray(); } }

                public void AddLineItem(OrderLine orderLine)
                {
                    _lineItems.Add(orderLine);
                }
            }

            public class OrderLine
            {
                public int Quantity { get; set; }
            }

            public class OrderDto
            {
                public OrderLineDto[] LineItems { get; set; }
            }

            public class OrderLineDto
            {
                public int Quantity { get; set; }
            }

            [Test]
            public void Example()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Order, OrderDto>();
                    cfg.CreateMap<OrderLine, OrderLineDto>();
                });

                var order = new Order();
                order.AddLineItem(new OrderLine { Quantity = 5 });
                order.AddLineItem(new OrderLine { Quantity = 15 });
                order.AddLineItem(new OrderLine { Quantity = 25 });

                var orderDto = config.CreateMapper().Map<Order, OrderDto>(order);

                orderDto.LineItems.Length.ShouldBe(3);
                orderDto.LineItems[0].Quantity.ShouldBe(5);
                orderDto.LineItems[1].Quantity.ShouldBe(15);
                orderDto.LineItems[2].Quantity.ShouldBe(25);
            }
        }
    }
}