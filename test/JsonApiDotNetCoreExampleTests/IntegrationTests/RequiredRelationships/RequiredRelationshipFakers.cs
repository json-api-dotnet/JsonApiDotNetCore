using System;
using Bogus;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    internal sealed class RequiredRelationshipFakers : FakerContainer
    {
        private readonly Lazy<Faker<Order>> _orderFaker = new Lazy<Faker<Order>>(() =>
            new Faker<Order>()
                .UseSeed(GetFakerSeed())
                .RuleFor(order => order.Value, f => f.Finance.Amount())
            );

        private readonly Lazy<Faker<Customer>> _customerFaker = new Lazy<Faker<Customer>>(() =>
            new Faker<Customer>()
                .UseSeed(GetFakerSeed())
                .RuleFor(customer => customer.Address, f => f.Lorem.Word())
            );

        private readonly Lazy<Faker<Delivery>> _deliveryFaker = new Lazy<Faker<Delivery>>(() =>
            new Faker<Delivery>()
                .UseSeed(GetFakerSeed())
                .RuleFor(delivery => delivery.TrackAndTraceCode, f => f.Lorem.Sentence())
                .RuleFor(delivery => delivery.ShippedAt, _ => DateTime.Now)
            );

        public Faker<Order> Orders => _orderFaker.Value;
        public Faker<Customer> Customers => _customerFaker.Value;
        public Faker<Delivery> Deliveries => _deliveryFaker.Value;
    }
}
