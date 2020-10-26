using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace UnitTests
{
    public sealed class CoreJsonApiControllerTests : CoreJsonApiController
    {
        [Fact]
        public void Errors_Correctly_Infers_Status_Code()
        {
            // Arrange
            var errors422 = new List<Error>
            {
                new Error(HttpStatusCode.UnprocessableEntity) {Title = "bad specific"},
                new Error(HttpStatusCode.UnprocessableEntity) {Title = "bad other specific"}
            };

            var errors400 = new List<Error>
            {
                new Error(HttpStatusCode.OK) {Title = "weird"},
                new Error(HttpStatusCode.BadRequest) {Title = "bad"},
                new Error(HttpStatusCode.UnprocessableEntity) {Title = "bad specific"}
            };

            var errors500 = new List<Error>
            {
                new Error(HttpStatusCode.OK) {Title = "weird"},
                new Error(HttpStatusCode.BadRequest) {Title = "bad"},
                new Error(HttpStatusCode.UnprocessableEntity) {Title = "bad specific"},
                new Error(HttpStatusCode.InternalServerError) {Title = "really bad"},
                new Error(HttpStatusCode.BadGateway) {Title = "really bad specific"}
            };
            
            // Act
            var result422 = Error(errors422);
            var result400 = Error(errors400);
            var result500 = Error(errors500);
            
            // Assert
            var response422 = Assert.IsType<ObjectResult>(result422);
            var response400 = Assert.IsType<ObjectResult>(result400);
            var response500 = Assert.IsType<ObjectResult>(result500);

            Assert.Equal((int)HttpStatusCode.UnprocessableEntity, response422.StatusCode);
            Assert.Equal((int)HttpStatusCode.BadRequest, response400.StatusCode);
            Assert.Equal((int)HttpStatusCode.InternalServerError, response500.StatusCode);
        }
    }
}
