using System;
using Bogus;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    internal sealed class DefaultBehaviorFakers : FakerContainer
    {
        private readonly Lazy<Faker<Order>> _orderFaker = new Lazy<Faker<Order>>(() =>
            new Faker<Order>()
                .UseSeed(GetFakerSeed())
                .RuleFor(order => order.Value, f => f.Finance.Amount())
        );

        private readonly Lazy<Faker<Customer>> _customerFaker = new Lazy<Faker<Customer>>(() =>
            new Faker<Customer>()
                .UseSeed(GetFakerSeed())
                .RuleFor(customer => customer.EmailAddress, f => f.Lorem.Word())
        );

        private readonly Lazy<Faker<Shipment>> _shipmentFaker = new Lazy<Faker<Shipment>>(() =>
            new Faker<Shipment>()
                .UseSeed(GetFakerSeed())
                .RuleFor(shipment => shipment.TrackAndTraceCode, f => f.Lorem.Sentence())
                .RuleFor(shipment => shipment.ShippedAt, _ => DateTime.Now)
        );

        public Faker<Order> Orders => _orderFaker.Value;
        public Faker<Customer> Customers => _customerFaker.Value;
        public Faker<Shipment> Shipments => _shipmentFaker.Value;
    }
}
