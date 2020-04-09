// TODO: Why is this file commented out?

//using JsonApiDotNetCore.Builders;
//using JsonApiDotNetCore.Configuration;
//using JsonApiDotNetCore.Services;
//using Microsoft.AspNetCore.Http;
//using Moq;
//using Xunit;

//namespace UnitTests.Builders
//{
//    public class BaseDocumentBuilderBehaviour_Tests
//    {

//        [Theory]
//        [InlineData(null, null, null, false)]
//        [InlineData(false, null, null, false)]
//        [InlineData(true, null, null, true)]
//        [InlineData(false, false, "true", false)]
//        [InlineData(false, true, "true", true)]
//        [InlineData(true, true, "false", false)]
//        [InlineData(true, false, "false", true)]
//        [InlineData(null, false, "false", false)]
//        [InlineData(null, false, "true", false)]
//        [InlineData(null, true, "true", true)]
//        [InlineData(null, true, "false", false)]
//        [InlineData(null, true, "foo", false)]
//        [InlineData(null, false, "foo", false)]
//        [InlineData(true, true, "foo", true)]
//        [InlineData(true, false, "foo", true)]
//        [InlineData(null, true, null, false)]
//        [InlineData(null, false, null, false)]
//        public void CheckNullBehaviorCombination(bool? omitNullValuedAttributes, bool? allowClientOverride, string clientOverride, bool omitsNulls)
//        {

//            NullAttributeResponseBehavior nullAttributeResponseBehavior; 
//            if (omitNullValuedAttributes.HasValue && allowClientOverride.HasValue)
//            {
//                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(omitNullValuedAttributes.Value, allowClientOverride.Value);
//            }else if (omitNullValuedAttributes.HasValue)
//            {
//                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(omitNullValuedAttributes.Value);
//            }else if
//                (allowClientOverride.HasValue)
//            {
//                nullAttributeResponseBehavior = new NullAttributeResponseBehavior(allowClientOverride: allowClientOverride.Value);
//            }
//            else
//            {
//                nullAttributeResponseBehavior = new NullAttributeResponseBehavior();
//            }

//            var jsonApiContextMock = new Mock<IJsonApiContext>();
//            jsonApiContextMock.SetupGet(m => m.Options)
//                .Returns(new JsonApiOptions() {NullAttributeResponseBehavior = nullAttributeResponseBehavior});

//            var httpContext = new DefaultHttpContext();
//            if (clientOverride != null)
//            {
//                httpContext.Request.QueryString = new QueryString($"?omitNullValuedAttributes={clientOverride}");
//            }
//            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
//            httpContextAccessorMock.SetupGet(m => m.HttpContext).Returns(httpContext);

//            var sut = new BaseDocumentBuilderOptionsProvider(jsonApiContextMock.Object, httpContextAccessorMock.Object);
//            var documentBuilderOptions = sut.GetBaseDocumentBuilderOptions();

//            Assert.Equal(omitsNulls, documentBuilderOptions.OmitNullValuedAttributes);
//        }

//    }
//}
