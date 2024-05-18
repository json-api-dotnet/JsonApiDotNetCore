using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

internal sealed class ObfuscationFakers
{
    private readonly Lazy<Faker<BankAccount>> _lazyBankAccountFaker = new(() => new Faker<BankAccount>()
        .MakeDeterministic()
        .RuleFor(bankAccount => bankAccount.Iban, faker => faker.Finance.Iban()));

    private readonly Lazy<Faker<DebitCard>> _lazyDebitCardFaker = new(() => new Faker<DebitCard>()
        .MakeDeterministic()
        .RuleFor(debitCard => debitCard.OwnerName, faker => faker.Name.FullName())
        .RuleFor(debitCard => debitCard.PinCode, faker => (short)faker.Random.Number(1000, 9999)));

    public Faker<BankAccount> BankAccount => _lazyBankAccountFaker.Value;
    public Faker<DebitCard> DebitCard => _lazyDebitCardFaker.Value;
}
