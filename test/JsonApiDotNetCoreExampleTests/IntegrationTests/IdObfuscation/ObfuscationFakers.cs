using System;
using Bogus;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    internal sealed class ObfuscationFakers : FakerContainer
    {
        private readonly Lazy<Faker<BankAccount>> _lazyBankAccountFaker = new Lazy<Faker<BankAccount>>(() =>
            new Faker<BankAccount>()
                .UseSeed(GetFakerSeed())
                .RuleFor(bankAccount => bankAccount.Iban, f => f.Finance.Iban()));

        private readonly Lazy<Faker<DebitCard>> _lazyDebitCardFaker = new Lazy<Faker<DebitCard>>(() =>
            new Faker<DebitCard>()
                .UseSeed(GetFakerSeed())
                .RuleFor(debitCard => debitCard.OwnerName, f => f.Name.FullName())
                .RuleFor(debitCard => debitCard.PinCode, f => (short)f.Random.Number(1000, 9999)));

        public Faker<BankAccount> BankAccount => _lazyBankAccountFaker.Value;
        public Faker<DebitCard> DebitCard => _lazyDebitCardFaker.Value;
    }
}
