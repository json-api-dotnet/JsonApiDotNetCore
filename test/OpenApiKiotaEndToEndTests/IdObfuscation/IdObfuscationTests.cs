using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.IdObfuscation.GeneratedCode;
using OpenApiKiotaEndToEndTests.IdObfuscation.GeneratedCode.Models;
using OpenApiTests.IdObfuscation;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.IdObfuscation;

public sealed class IdObfuscationTests : IClassFixture<IntegrationTestContext<ObfuscationStartup, ObfuscationDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<ObfuscationStartup, ObfuscationDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ObfuscationFakers _fakers = new();

    public IdObfuscationTests(IntegrationTestContext<ObfuscationStartup, ObfuscationDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<BankAccountsController>();
        testContext.UseController<DebitCardsController>();
        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Can_get_primary_resources()
    {
        // Arrange
        List<BankAccount> accounts = _fakers.BankAccount.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<BankAccount>();
            dbContext.BankAccounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        // Act
        BankAccountCollectionResponseDocument? response = await apiClient.BankAccounts.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.Should().ContainSingle(data => data.Id == accounts[0].StringId);
        response.Data.Should().ContainSingle(data => data.Id == accounts[1].StringId);
    }

    [Fact]
    public async Task Cannot_get_primary_resource_for_invalid_ID()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        // Act
        Func<Task> action = async () => _ = await apiClient.BankAccounts["not-a-hex-value"].GetAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors[0];
        error.Status.Should().Be("400");
        error.Title.Should().Be("Invalid ID value.");
        error.Detail.Should().Be("The value 'not-a-hex-value' is not a valid hexadecimal value.");
    }

    [Fact]
    public async Task Can_get_primary_resource_by_ID()
    {
        // Arrange
        BankAccount account = _fakers.BankAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.BankAccounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        // Act
        PrimaryBankAccountResponseDocument? response = await apiClient.BankAccounts[account.StringId!].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(account.StringId);
    }

    [Fact]
    public async Task Can_get_secondary_resources()
    {
        // Arrange
        BankAccount account = _fakers.BankAccount.GenerateOne();
        account.Cards = _fakers.DebitCard.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.BankAccounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        // Act
        DebitCardCollectionResponseDocument? response = await apiClient.BankAccounts[account.StringId!].Cards.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().HaveCount(2);

        response.Data.Should().ContainSingle(data => data.Id == account.Cards[0].StringId);
        response.Data.Should().ContainSingle(data => data.Id == account.Cards[1].StringId);
    }

    [Fact]
    public async Task Can_include_resource_with_sparse_fieldset()
    {
        // Arrange
        BankAccount account = _fakers.BankAccount.GenerateOne();
        account.Cards = _fakers.DebitCard.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.BankAccounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        using IDisposable scope = _requestAdapterFactory.WithQueryString(new Dictionary<string, string?>
        {
            ["include"] = "cards",
            ["fields[debitCards]"] = "ownerName"
        });

        // Act
        PrimaryBankAccountResponseDocument? response = await apiClient.BankAccounts[account.StringId!].GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(account.StringId);

        response.Data.Relationships.Should().NotBeNull();
        response.Data.Relationships.Cards.Should().NotBeNull();
        response.Data.Relationships.Cards.Data.Should().ContainSingle().Which.Id.Should().Be(account.Cards[0].StringId);

        response.Included.Should().HaveCount(1);

        response.Included.OfType<DataInDebitCardResponse>().Should().ContainSingle().Which.With(include =>
        {
            include.Id.Should().Be(account.Cards[0].StringId);
            include.Attributes.Should().NotBeNull();
            include.Attributes.OwnerName.Should().Be(account.Cards[0].OwnerName);
            include.Attributes.PinCode.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_get_relationship()
    {
        // Arrange
        BankAccount account = _fakers.BankAccount.GenerateOne();
        account.Cards = _fakers.DebitCard.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.BankAccounts.Add(account);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        // Act
        DebitCardIdentifierCollectionResponseDocument? response = await apiClient.BankAccounts[account.StringId!].Relationships.Cards.GetAsync();

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().ContainSingle().Which.Id.Should().Be(account.Cards[0].StringId);
    }

    [Fact]
    public async Task Can_create_resource_with_relationship()
    {
        // Arrange
        BankAccount existingAccount = _fakers.BankAccount.GenerateOne();
        DebitCard newCard = _fakers.DebitCard.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.BankAccounts.Add(existingAccount);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        var requestBody = new CreateDebitCardRequestDocument
        {
            Data = new DataInCreateDebitCardRequest
            {
                Type = DebitCardResourceType.DebitCards,
                Attributes = new AttributesInCreateDebitCardRequest
                {
                    OwnerName = newCard.OwnerName,
                    PinCode = newCard.PinCode
                },
                Relationships = new RelationshipsInCreateDebitCardRequest
                {
                    Account = new ToOneBankAccountInRequest
                    {
                        Data = new BankAccountIdentifierInRequest
                        {
                            Type = ResourceType.BankAccounts,
                            Id = existingAccount.StringId
                        }
                    }
                }
            }
        };

        // Act
        PrimaryDebitCardResponseDocument? response = await apiClient.DebitCards.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.Attributes.Should().NotBeNull();
        response.Data.Attributes.OwnerName.Should().Be(newCard.OwnerName);
        response.Data.Attributes.PinCode.Should().Be(newCard.PinCode);

        response.Data.Relationships.Should().NotBeNull();
        response.Data.Relationships.Account.Should().NotBeNull();

        long newCardId = HexadecimalCodec.Instance.Decode(response.Data.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            DebitCard cardInDatabase = await dbContext.DebitCards.Include(card => card.Account).FirstWithIdAsync(newCardId);

            cardInDatabase.OwnerName.Should().Be(newCard.OwnerName);
            cardInDatabase.PinCode.Should().Be(newCard.PinCode);

            cardInDatabase.Account.Should().NotBeNull();
            cardInDatabase.Account.Id.Should().Be(existingAccount.Id);
            cardInDatabase.Account.StringId.Should().Be(existingAccount.StringId);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_relationship()
    {
        // Arrange
        BankAccount existingAccount = _fakers.BankAccount.GenerateOne();
        existingAccount.Cards = _fakers.DebitCard.GenerateList(1);

        DebitCard existingCard = _fakers.DebitCard.GenerateOne();
        existingCard.Account = _fakers.BankAccount.GenerateOne();

        string newIban = _fakers.BankAccount.GenerateOne().Iban;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingAccount, existingCard);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        var requestBody = new UpdateBankAccountRequestDocument
        {
            Data = new DataInUpdateBankAccountRequest
            {
                Type = BankAccountResourceType.BankAccounts,
                Id = existingAccount.StringId,
                Attributes = new AttributesInUpdateBankAccountRequest
                {
                    Iban = newIban
                },
                Relationships = new RelationshipsInUpdateBankAccountRequest
                {
                    Cards = new ToManyDebitCardInRequest
                    {
                        Data =
                        [
                            new DebitCardIdentifierInRequest
                            {
                                Type = ResourceType.DebitCards,
                                Id = existingCard.StringId
                            }
                        ]
                    }
                }
            }
        };

        // Act
        PrimaryBankAccountResponseDocument? response = await apiClient.BankAccounts[existingAccount.StringId!].PatchAsync(requestBody);

        // Assert
        response.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            BankAccount accountInDatabase = await dbContext.BankAccounts.Include(account => account.Cards).FirstWithIdAsync(existingAccount.Id);

            accountInDatabase.Iban.Should().Be(newIban);

            accountInDatabase.Cards.Should().HaveCount(1);
            accountInDatabase.Cards[0].Id.Should().Be(existingCard.Id);
            accountInDatabase.Cards[0].StringId.Should().Be(existingCard.StringId);
        });
    }

    [Fact]
    public async Task Can_add_to_ToMany_relationship()
    {
        // Arrange
        BankAccount existingAccount = _fakers.BankAccount.GenerateOne();
        existingAccount.Cards = _fakers.DebitCard.GenerateList(1);

        DebitCard existingDebitCard = _fakers.DebitCard.GenerateOne();
        existingDebitCard.Account = _fakers.BankAccount.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingAccount, existingDebitCard);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        var requestBody = new ToManyDebitCardInRequest
        {
            Data =
            [
                new DebitCardIdentifierInRequest
                {
                    Type = ResourceType.DebitCards,
                    Id = existingDebitCard.StringId
                }
            ]
        };

        // Act
        await apiClient.BankAccounts[existingAccount.StringId!].Relationships.Cards.PostAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            BankAccount accountInDatabase = await dbContext.BankAccounts.Include(account => account.Cards).FirstWithIdAsync(existingAccount.Id);

            accountInDatabase.Cards.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task Can_remove_from_ToMany_relationship()
    {
        // Arrange
        BankAccount existingAccount = _fakers.BankAccount.GenerateOne();
        existingAccount.Cards = _fakers.DebitCard.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.BankAccounts.Add(existingAccount);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        var requestBody = new ToManyDebitCardInRequest
        {
            Data =
            [
                new DebitCardIdentifierInRequest
                {
                    Type = ResourceType.DebitCards,
                    Id = existingAccount.Cards[0].StringId
                }
            ]
        };

        // Act
        await apiClient.BankAccounts[existingAccount.StringId!].Relationships.Cards.DeleteAsync(requestBody);

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            BankAccount accountInDatabase = await dbContext.BankAccounts.Include(account => account.Cards).FirstWithIdAsync(existingAccount.Id);

            accountInDatabase.Cards.Should().HaveCount(1);
        });
    }

    [Fact]
    public async Task Can_delete_resource()
    {
        // Arrange
        BankAccount existingAccount = _fakers.BankAccount.GenerateOne();
        existingAccount.Cards = _fakers.DebitCard.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.BankAccounts.Add(existingAccount);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        // Act
        await apiClient.BankAccounts[existingAccount.StringId!].DeleteAsync();

        // Assert
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            BankAccount? accountInDatabase = await dbContext.BankAccounts.Include(account => account.Cards).FirstWithIdOrDefaultAsync(existingAccount.Id);

            accountInDatabase.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_delete_unknown_resource()
    {
        // Arrange
        string stringId = HexadecimalCodec.Instance.Encode(Unknown.TypedId.Int64)!;

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        // Act
        Func<Task> action = async () => await apiClient.BankAccounts[stringId].DeleteAsync();

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'bankAccounts' with ID '{stringId}' does not exist.");
    }

    [Fact]
    public async Task Can_use_operations()
    {
        // Arrange
        BankAccount newAccount = _fakers.BankAccount.GenerateOne();
        DebitCard newCard = _fakers.DebitCard.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new IdObfuscationClient(requestAdapter);

        const string accountLocalId = "new-bank-account";

        var requestBody = new OperationsRequestDocument
        {
            AtomicOperations =
            [
                new CreateBankAccountOperation
                {
                    Data = new DataInCreateBankAccountRequest
                    {
                        Type = BankAccountResourceType.BankAccounts,
                        Lid = accountLocalId,
                        Attributes = new AttributesInCreateBankAccountRequest
                        {
                            Iban = newAccount.Iban
                        }
                    }
                },
                new CreateDebitCardOperation
                {
                    Data = new DataInCreateDebitCardRequest
                    {
                        Type = DebitCardResourceType.DebitCards,
                        Attributes = new AttributesInCreateDebitCardRequest
                        {
                            OwnerName = newCard.OwnerName,
                            PinCode = newCard.PinCode
                        },
                        Relationships = new RelationshipsInCreateDebitCardRequest
                        {
                            Account = new ToOneBankAccountInRequest
                            {
                                Data = new BankAccountIdentifierInRequest
                                {
                                    Type = ResourceType.BankAccounts,
                                    Lid = accountLocalId
                                }
                            }
                        }
                    }
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await apiClient.Operations.PostAsync(requestBody);

        // Assert
        response.Should().NotBeNull();
        response.AtomicResults.Should().HaveCount(2);

        DataInBankAccountResponse accountData = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<DataInBankAccountResponse>().Subject!;
        accountData.Relationships.Should().NotBeNull();
        accountData.Relationships.Cards.Should().NotBeNull();

        DataInDebitCardResponse cardData = response.AtomicResults.ElementAt(1).Data.Should().BeOfType<DataInDebitCardResponse>().Subject!;
        cardData.Relationships.Should().NotBeNull();
        cardData.Relationships.Account.Should().NotBeNull();

        long newAccountId = HexadecimalCodec.Instance.Decode(accountData.Id);
        long newCardId = HexadecimalCodec.Instance.Decode(cardData.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            DebitCard cardInDatabase = await dbContext.DebitCards.Include(card => card.Account).FirstWithIdAsync(newCardId);

            cardInDatabase.OwnerName.Should().Be(newCard.OwnerName);
            cardInDatabase.PinCode.Should().Be(newCard.PinCode);

            cardInDatabase.Account.Should().NotBeNull();
            cardInDatabase.Account.Id.Should().Be(newAccountId);
            cardInDatabase.Account.Iban.Should().Be(newAccount.Iban);
        });
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
