using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace UnitTests
{
    public class JsonApiControllerMixin_Tests : JsonApiControllerMixin
    {

        [Fact]
        public void Errors_Correctly_Infers_Status_Code()
        {
            // arrange
            var errors422 = new ErrorCollection {
                Errors = new List<Error> {
                    new Error(422, "bad specific"),
                    new Error(422, "bad other specific"),
                }
            };

            var errors400 = new ErrorCollection {
                Errors = new List<Error> {
                    new Error(200, "weird"),
                    new Error(400, "bad"),
                    new Error(422, "bad specific"),
                }
            };

            var errors500 = new ErrorCollection {
                Errors = new List<Error> {
                    new Error(200, "weird"),
                    new Error(400, "bad"),
                    new Error(422, "bad specific"),
                    new Error(500, "really bad"),
                    new Error(502, "really bad specific"),
                }
            };
            
            // act
            var result422 = this.Errors(errors422);
            var result400 = this.Errors(errors400);
            var result500 = this.Errors(errors500);
            
            // assert
            var response422 = Assert.IsType<ObjectResult>(result422);
            var response400 = Assert.IsType<ObjectResult>(result400);
            var response500 = Assert.IsType<ObjectResult>(result500);

            Assert.Equal(422, response422.StatusCode);
            Assert.Equal(400, response400.StatusCode);
            Assert.Equal(500, response500.StatusCode);
        }
    }
}
