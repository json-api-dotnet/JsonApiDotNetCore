using System.Net;
using System.Threading.Tasks;
using Bogus;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExampleTests.Acceptance.Spec;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance
{
    public class KebabCaseFormatterTests : FunctionalTestCollection<KebabCaseApplicationFactory>
    {
        private readonly Faker<KebabCasedModel> _faker;

        public KebabCaseFormatterTests(KebabCaseApplicationFactory factory) : base(factory)
        {
            _faker = new Faker<KebabCasedModel>().RuleFor(m => m.CompoundAttr, f => f.Lorem.Sentence());
        }

        [Fact]
        public async Task KebabCaseFormatter_GetAll_IsReturned()
        {
            // Arrange
            var model = _faker.Generate();
            _dbContext.KebabCasedModels.Add(model);
            _dbContext.SaveChanges();

            // Act
            var (body, response) = await Get("api/v1/kebab-cased-models");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            var responseItem = _deserializer.DeserializeList<KebabCasedModel>(body).Data;
            Assert.True(responseItem.Count > 0);
        }

        [Fact]
        public async Task KebabCaseFormatter_GetSingle_IsReturned()
        {
            // Arrange
            var model = _faker.Generate();
            _dbContext.KebabCasedModels.Add(model);
            _dbContext.SaveChanges();

            // Act
            var (body, response) = await Get($"api/v1/kebab-cased-models/{model.Id}");

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            var responseItem = _deserializer.DeserializeSingle<KebabCasedModel>(body).Data;
            Assert.Equal(model.Id, responseItem.Id);
        }

        [Fact]
        public async Task KebabCaseFormatter_Create_IsCreated()
        {
            // Arrange
            var model = _faker.Generate();
            var serializer = GetSerializer<KebabCasedModel>(kcm => new { kcm.CompoundAttr });

            // Act
            var (body, response) = await Post("api/v1/kebab-cased-models", serializer.Serialize(model));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.Created, response);
            var responseItem = _deserializer.DeserializeSingle<KebabCasedModel>(body).Data;
            Assert.Equal(model.CompoundAttr, responseItem.CompoundAttr);
        }

        [Fact]
        public async Task KebabCaseFormatter_Update_IsUpdated()
        {
            // Arrange
            var model = _faker.Generate();
            _dbContext.KebabCasedModels.Add(model);
            _dbContext.SaveChanges();
            model.CompoundAttr = _faker.Generate().CompoundAttr;
            var serializer = GetSerializer<KebabCasedModel>(kcm => new { kcm.CompoundAttr });

            // Act
            var (body, response) = await Patch($"api/v1/kebab-cased-models/{model.Id}", serializer.Serialize(model));

            // Assert
            AssertEqualStatusCode(HttpStatusCode.OK, response);
            var responseItem = _deserializer.DeserializeSingle<KebabCasedModel>(body).Data;
            Assert.Equal(model.CompoundAttr, responseItem.CompoundAttr);
        }
    }
}
