// // Decompiled with JetBrains decompiler
// // Type: ResourcesInternal
// // Assembly: Microsoft.AspNetCore.Mvc.Core, Version=3.1.5.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// // MVID: C072ADA3-420F-4E08-8493-043C3240EB59
// // Assembly location: /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App/3.1.5/Microsoft.AspNetCore.Mvc.Core.dll
//
// using System;
// using System.Globalization;
// using System.Resources;
// using System.Runtime.CompilerServices;
//
// namespace JsonApiDotNetCore.Configuration
// {
//   internal static class ResourcesInternal
//   {
//     private static ResourceManager s_resourceManager;
//
//     internal static ResourceManager ResourceManager
//     {
//       get
//       {
//         return s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof (ResourcesInternal)));
//       }
//     }
//
//     internal static CultureInfo Culture { get; set; }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     internal static string GetResourceString(string resourceKey, string defaultValue = null)
//     {
//       return ResourcesInternal.ResourceManager.GetString(resourceKey, ResourcesInternal.Culture);
//     }
//
//     private static string GetResourceString(string resourceKey, string[] formatterNames)
//     {
//       string str = ResourcesInternal.GetResourceString(resourceKey, (string) null);
//       if (formatterNames != null)
//       {
//         for (int index = 0; index < formatterNames.Length; ++index)
//           str = str.Replace("{" + formatterNames[index] + "}", "{" + index.ToString() + "}");
//       }
//       return str;
//     }
//
//     internal static string MatchAllContentTypeIsNotAllowed
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (MatchAllContentTypeIsNotAllowed), (string) null);
//       }
//     }
//
//     internal static string FormatMatchAllContentTypeIsNotAllowed(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("MatchAllContentTypeIsNotAllowed", (string) null), p0);
//     }
//
//     internal static string ObjectResult_MatchAllContentType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ObjectResult_MatchAllContentType), (string) null);
//       }
//     }
//
//     internal static string FormatObjectResult_MatchAllContentType(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ObjectResult_MatchAllContentType", (string) null), p0, p1);
//     }
//
//     internal static string ActionExecutor_WrappedTaskInstance
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ActionExecutor_WrappedTaskInstance), (string) null);
//       }
//     }
//
//     internal static string FormatActionExecutor_WrappedTaskInstance(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ActionExecutor_WrappedTaskInstance", (string) null), p0, p1, p2);
//     }
//
//     internal static string ActionExecutor_UnexpectedTaskInstance
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ActionExecutor_UnexpectedTaskInstance), (string) null);
//       }
//     }
//
//     internal static string FormatActionExecutor_UnexpectedTaskInstance(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ActionExecutor_UnexpectedTaskInstance", (string) null), p0, p1);
//     }
//
//     internal static string ActionInvokerFactory_CouldNotCreateInvoker
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ActionInvokerFactory_CouldNotCreateInvoker), (string) null);
//       }
//     }
//
//     internal static string FormatActionInvokerFactory_CouldNotCreateInvoker(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ActionInvokerFactory_CouldNotCreateInvoker", (string) null), p0);
//     }
//
//     internal static string ActionDescriptorMustBeBasedOnControllerAction
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ActionDescriptorMustBeBasedOnControllerAction), (string) null);
//       }
//     }
//
//     internal static string FormatActionDescriptorMustBeBasedOnControllerAction(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ActionDescriptorMustBeBasedOnControllerAction", (string) null), p0);
//     }
//
//     internal static string ArgumentCannotBeNullOrEmpty
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ArgumentCannotBeNullOrEmpty), (string) null);
//       }
//     }
//
//     internal static string PropertyOfTypeCannotBeNull
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (PropertyOfTypeCannotBeNull), (string) null);
//       }
//     }
//
//     internal static string FormatPropertyOfTypeCannotBeNull(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("PropertyOfTypeCannotBeNull", (string) null), p0, p1);
//     }
//
//     internal static string TypeMethodMustReturnNotNullValue
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (TypeMethodMustReturnNotNullValue), (string) null);
//       }
//     }
//
//     internal static string FormatTypeMethodMustReturnNotNullValue(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("TypeMethodMustReturnNotNullValue", (string) null), p0, p1);
//     }
//
//     internal static string ModelBinding_NullValueNotValid
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinding_NullValueNotValid), (string) null);
//       }
//     }
//
//     internal static string FormatModelBinding_NullValueNotValid(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelBinding_NullValueNotValid", (string) null), p0);
//     }
//
//     internal static string Invalid_IncludePropertyExpression
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (Invalid_IncludePropertyExpression), (string) null);
//       }
//     }
//
//     internal static string FormatInvalid_IncludePropertyExpression(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("Invalid_IncludePropertyExpression", (string) null), p0);
//     }
//
//     internal static string NoRoutesMatched
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (NoRoutesMatched), (string) null);
//       }
//     }
//
//     internal static string AsyncActionFilter_InvalidShortCircuit
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AsyncActionFilter_InvalidShortCircuit), (string) null);
//       }
//     }
//
//     internal static string FormatAsyncActionFilter_InvalidShortCircuit(
//       object p0,
//       object p1,
//       object p2,
//       object p3)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AsyncActionFilter_InvalidShortCircuit", (string) null), p0, p1, p2, p3);
//     }
//
//     internal static string AsyncResultFilter_InvalidShortCircuit
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AsyncResultFilter_InvalidShortCircuit), (string) null);
//       }
//     }
//
//     internal static string FormatAsyncResultFilter_InvalidShortCircuit(
//       object p0,
//       object p1,
//       object p2,
//       object p3)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AsyncResultFilter_InvalidShortCircuit", (string) null), p0, p1, p2, p3);
//     }
//
//     internal static string FilterFactoryAttribute_TypeMustImplementIFilter
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (FilterFactoryAttribute_TypeMustImplementIFilter), (string) null);
//       }
//     }
//
//     internal static string FormatFilterFactoryAttribute_TypeMustImplementIFilter(
//       object p0,
//       object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("FilterFactoryAttribute_TypeMustImplementIFilter", (string) null), p0, p1);
//     }
//
//     internal static string ActionResult_ActionReturnValueCannotBeNull
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ActionResult_ActionReturnValueCannotBeNull), (string) null);
//       }
//     }
//
//     internal static string FormatActionResult_ActionReturnValueCannotBeNull(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ActionResult_ActionReturnValueCannotBeNull", (string) null), p0);
//     }
//
//     internal static string TypeMustDeriveFromType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (TypeMustDeriveFromType), (string) null);
//       }
//     }
//
//     internal static string FormatTypeMustDeriveFromType(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("TypeMustDeriveFromType", (string) null), p0, p1);
//     }
//
//     internal static string InputFormatterNoEncoding
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (InputFormatterNoEncoding), (string) null);
//       }
//     }
//
//     internal static string FormatInputFormatterNoEncoding(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("InputFormatterNoEncoding", (string) null), p0);
//     }
//
//     internal static string UnsupportedContentType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (UnsupportedContentType), (string) null);
//       }
//     }
//
//     internal static string FormatUnsupportedContentType(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("UnsupportedContentType", (string) null), p0);
//     }
//
//     internal static string OutputFormatterNoMediaType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (OutputFormatterNoMediaType), (string) null);
//       }
//     }
//
//     internal static string FormatOutputFormatterNoMediaType(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("OutputFormatterNoMediaType", (string) null), p0);
//     }
//
//     internal static string AttributeRoute_AggregateErrorMessage
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_AggregateErrorMessage), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_AggregateErrorMessage(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_AggregateErrorMessage", (string) null), p0, p1);
//     }
//
//     internal static string AttributeRoute_CannotContainParameter
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_CannotContainParameter), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_CannotContainParameter(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_CannotContainParameter", (string) null), p0, p1, p2);
//     }
//
//     internal static string AttributeRoute_IndividualErrorMessage
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_IndividualErrorMessage), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_IndividualErrorMessage(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_IndividualErrorMessage", (string) null), p0, p1, p2);
//     }
//
//     internal static string AttributeRoute_TokenReplacement_EmptyTokenNotAllowed
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_TokenReplacement_EmptyTokenNotAllowed), (string) null);
//       }
//     }
//
//     internal static string AttributeRoute_TokenReplacement_ImbalancedSquareBrackets
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_TokenReplacement_ImbalancedSquareBrackets), (string) null);
//       }
//     }
//
//     internal static string AttributeRoute_TokenReplacement_InvalidSyntax
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_TokenReplacement_InvalidSyntax), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_TokenReplacement_InvalidSyntax(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_TokenReplacement_InvalidSyntax", (string) null), p0, p1);
//     }
//
//     internal static string AttributeRoute_TokenReplacement_ReplacementValueNotFound
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_TokenReplacement_ReplacementValueNotFound), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_TokenReplacement_ReplacementValueNotFound(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_TokenReplacement_ReplacementValueNotFound", (string) null), p0, p1, p2);
//     }
//
//     internal static string AttributeRoute_TokenReplacement_UnclosedToken
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_TokenReplacement_UnclosedToken), (string) null);
//       }
//     }
//
//     internal static string AttributeRoute_TokenReplacement_UnescapedBraceInToken
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_TokenReplacement_UnescapedBraceInToken), (string) null);
//       }
//     }
//
//     internal static string UnableToFindServices
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (UnableToFindServices), (string) null);
//       }
//     }
//
//     internal static string FormatUnableToFindServices(object p0, object p1, object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("UnableToFindServices", (string) null), p0, p1, p2);
//     }
//
//     internal static string AttributeRoute_DuplicateNames_Item
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_DuplicateNames_Item), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_DuplicateNames_Item(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_DuplicateNames_Item", (string) null), p0, p1);
//     }
//
//     internal static string AttributeRoute_DuplicateNames
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_DuplicateNames), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_DuplicateNames(object p0, object p1, object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_DuplicateNames", (string) null), p0, p1, p2);
//     }
//
//     internal static string AttributeRoute_AggregateErrorMessage_ErrorNumber
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_AggregateErrorMessage_ErrorNumber), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_AggregateErrorMessage_ErrorNumber(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_AggregateErrorMessage_ErrorNumber", (string) null), p0, p1, p2);
//     }
//
//     internal static string AttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod", (string) null), p0, p1, p2);
//     }
//
//     internal static string AttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod_Item
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod_Item), (string) null);
//       }
//     }
//
//     internal static string FormatAttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod_Item(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod_Item", (string) null), p0, p1, p2);
//     }
//
//     internal static string AttributeRoute_NullTemplateRepresentation
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AttributeRoute_NullTemplateRepresentation), (string) null);
//       }
//     }
//
//     internal static string DefaultActionSelector_AmbiguousActions
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (DefaultActionSelector_AmbiguousActions), (string) null);
//       }
//     }
//
//     internal static string FormatDefaultActionSelector_AmbiguousActions(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("DefaultActionSelector_AmbiguousActions", (string) null), p0, p1);
//     }
//
//     internal static string FileResult_InvalidPath
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (FileResult_InvalidPath), (string) null);
//       }
//     }
//
//     internal static string FormatFileResult_InvalidPath(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("FileResult_InvalidPath", (string) null), p0);
//     }
//
//     internal static string SerializableError_DefaultError
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (SerializableError_DefaultError), (string) null);
//       }
//     }
//
//     internal static string AsyncResourceFilter_InvalidShortCircuit
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AsyncResourceFilter_InvalidShortCircuit), (string) null);
//       }
//     }
//
//     internal static string FormatAsyncResourceFilter_InvalidShortCircuit(
//       object p0,
//       object p1,
//       object p2,
//       object p3)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AsyncResourceFilter_InvalidShortCircuit", (string) null), p0, p1, p2, p3);
//     }
//
//     internal static string ResponseCache_SpecifyDuration
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ResponseCache_SpecifyDuration), (string) null);
//       }
//     }
//
//     internal static string FormatResponseCache_SpecifyDuration(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ResponseCache_SpecifyDuration", (string) null), p0, p1);
//     }
//
//     internal static string ApiExplorer_UnsupportedAction
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiExplorer_UnsupportedAction), (string) null);
//       }
//     }
//
//     internal static string FormatApiExplorer_UnsupportedAction(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApiExplorer_UnsupportedAction", (string) null), p0);
//     }
//
//     internal static string FormatterMappings_NotValidMediaType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (FormatterMappings_NotValidMediaType), (string) null);
//       }
//     }
//
//     internal static string FormatFormatterMappings_NotValidMediaType(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("FormatterMappings_NotValidMediaType", (string) null), p0);
//     }
//
//     internal static string Format_NotValid
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (Format_NotValid), (string) null);
//       }
//     }
//
//     internal static string FormatFormat_NotValid(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("Format_NotValid", (string) null), p0);
//     }
//
//     internal static string CacheProfileNotFound
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (CacheProfileNotFound), (string) null);
//       }
//     }
//
//     internal static string FormatCacheProfileNotFound(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("CacheProfileNotFound", (string) null), p0);
//     }
//
//     internal static string ModelType_WrongType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelType_WrongType), (string) null);
//       }
//     }
//
//     internal static string FormatModelType_WrongType(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelType_WrongType", (string) null), p0, p1);
//     }
//
//     internal static string ValueInterfaceAbstractOrOpenGenericTypesCannotBeActivated
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ValueInterfaceAbstractOrOpenGenericTypesCannotBeActivated), (string) null);
//       }
//     }
//
//     internal static string FormatValueInterfaceAbstractOrOpenGenericTypesCannotBeActivated(
//       object p0,
//       object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ValueInterfaceAbstractOrOpenGenericTypesCannotBeActivated", (string) null), p0, p1);
//     }
//
//     internal static string BinderType_MustBeIModelBinder
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (BinderType_MustBeIModelBinder), (string) null);
//       }
//     }
//
//     internal static string FormatBinderType_MustBeIModelBinder(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("BinderType_MustBeIModelBinder", (string) null), p0, p1);
//     }
//
//     internal static string BindingSource_CannotBeComposite
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (BindingSource_CannotBeComposite), (string) null);
//       }
//     }
//
//     internal static string FormatBindingSource_CannotBeComposite(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("BindingSource_CannotBeComposite", (string) null), p0, p1);
//     }
//
//     internal static string BindingSource_CannotBeGreedy
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (BindingSource_CannotBeGreedy), (string) null);
//       }
//     }
//
//     internal static string FormatBindingSource_CannotBeGreedy(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("BindingSource_CannotBeGreedy", (string) null), p0, p1);
//     }
//
//     internal static string Common_PropertyNotFound
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (Common_PropertyNotFound), (string) null);
//       }
//     }
//
//     internal static string FormatCommon_PropertyNotFound(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("Common_PropertyNotFound", (string) null), p0, p1);
//     }
//
//     internal static string JQueryFormValueProviderFactory_MissingClosingBracket
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (JQueryFormValueProviderFactory_MissingClosingBracket), (string) null);
//       }
//     }
//
//     internal static string FormatJQueryFormValueProviderFactory_MissingClosingBracket(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("JQueryFormValueProviderFactory_MissingClosingBracket", (string) null), p0);
//     }
//
//     internal static string KeyValuePair_BothKeyAndValueMustBePresent
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (KeyValuePair_BothKeyAndValueMustBePresent), (string) null);
//       }
//     }
//
//     internal static string ModelBinderUtil_ModelCannotBeNull
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinderUtil_ModelCannotBeNull), (string) null);
//       }
//     }
//
//     internal static string FormatModelBinderUtil_ModelCannotBeNull(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelBinderUtil_ModelCannotBeNull", (string) null), p0);
//     }
//
//     internal static string ModelBinderUtil_ModelInstanceIsWrong
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinderUtil_ModelInstanceIsWrong), (string) null);
//       }
//     }
//
//     internal static string FormatModelBinderUtil_ModelInstanceIsWrong(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelBinderUtil_ModelInstanceIsWrong", (string) null), p0, p1);
//     }
//
//     internal static string ModelBinderUtil_ModelMetadataCannotBeNull
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinderUtil_ModelMetadataCannotBeNull), (string) null);
//       }
//     }
//
//     internal static string ModelBinding_MissingBindRequiredMember
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinding_MissingBindRequiredMember), (string) null);
//       }
//     }
//
//     internal static string FormatModelBinding_MissingBindRequiredMember(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelBinding_MissingBindRequiredMember", (string) null), p0);
//     }
//
//     internal static string ModelBinding_MissingRequestBodyRequiredMember
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinding_MissingRequestBodyRequiredMember), (string) null);
//       }
//     }
//
//     internal static string ValueProviderResult_NoConverterExists
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ValueProviderResult_NoConverterExists), (string) null);
//       }
//     }
//
//     internal static string FormatValueProviderResult_NoConverterExists(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ValueProviderResult_NoConverterExists", (string) null), p0, p1);
//     }
//
//     internal static string FileResult_PathNotRooted
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (FileResult_PathNotRooted), (string) null);
//       }
//     }
//
//     internal static string FormatFileResult_PathNotRooted(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("FileResult_PathNotRooted", (string) null), p0);
//     }
//
//     internal static string UrlNotLocal
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (UrlNotLocal), (string) null);
//       }
//     }
//
//     internal static string FormatFormatterMappings_GetMediaTypeMappingForFormat_InvalidFormat
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (FormatFormatterMappings_GetMediaTypeMappingForFormat_InvalidFormat), (string) null);
//       }
//     }
//
//     internal static string FormatFormatFormatterMappings_GetMediaTypeMappingForFormat_InvalidFormat(
//       object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("FormatFormatterMappings_GetMediaTypeMappingForFormat_InvalidFormat", (string) null), p0);
//     }
//
//     internal static string AcceptHeaderParser_ParseAcceptHeader_InvalidValues
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AcceptHeaderParser_ParseAcceptHeader_InvalidValues), (string) null);
//       }
//     }
//
//     internal static string FormatAcceptHeaderParser_ParseAcceptHeader_InvalidValues(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AcceptHeaderParser_ParseAcceptHeader_InvalidValues", (string) null), p0);
//     }
//
//     internal static string ModelState_AttemptedValueIsInvalid
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelState_AttemptedValueIsInvalid), (string) null);
//       }
//     }
//
//     internal static string FormatModelState_AttemptedValueIsInvalid(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelState_AttemptedValueIsInvalid", (string) null), p0, p1);
//     }
//
//     internal static string ModelState_NonPropertyAttemptedValueIsInvalid
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelState_NonPropertyAttemptedValueIsInvalid), (string) null);
//       }
//     }
//
//     internal static string FormatModelState_NonPropertyAttemptedValueIsInvalid(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelState_NonPropertyAttemptedValueIsInvalid", (string) null), p0);
//     }
//
//     internal static string ModelState_UnknownValueIsInvalid
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelState_UnknownValueIsInvalid), (string) null);
//       }
//     }
//
//     internal static string FormatModelState_UnknownValueIsInvalid(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelState_UnknownValueIsInvalid", (string) null), p0);
//     }
//
//     internal static string ModelState_NonPropertyUnknownValueIsInvalid
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelState_NonPropertyUnknownValueIsInvalid), (string) null);
//       }
//     }
//
//     internal static string HtmlGeneration_ValueIsInvalid
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (HtmlGeneration_ValueIsInvalid), (string) null);
//       }
//     }
//
//     internal static string FormatHtmlGeneration_ValueIsInvalid(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("HtmlGeneration_ValueIsInvalid", (string) null), p0);
//     }
//
//     internal static string HtmlGeneration_ValueMustBeNumber
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (HtmlGeneration_ValueMustBeNumber), (string) null);
//       }
//     }
//
//     internal static string FormatHtmlGeneration_ValueMustBeNumber(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("HtmlGeneration_ValueMustBeNumber", (string) null), p0);
//     }
//
//     internal static string HtmlGeneration_NonPropertyValueMustBeNumber
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (HtmlGeneration_NonPropertyValueMustBeNumber), (string) null);
//       }
//     }
//
//     internal static string TextInputFormatter_SupportedEncodingsMustNotBeEmpty
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (TextInputFormatter_SupportedEncodingsMustNotBeEmpty), (string) null);
//       }
//     }
//
//     internal static string FormatTextInputFormatter_SupportedEncodingsMustNotBeEmpty(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("TextInputFormatter_SupportedEncodingsMustNotBeEmpty", (string) null), p0);
//     }
//
//     internal static string TextOutputFormatter_SupportedEncodingsMustNotBeEmpty
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (TextOutputFormatter_SupportedEncodingsMustNotBeEmpty), (string) null);
//       }
//     }
//
//     internal static string FormatTextOutputFormatter_SupportedEncodingsMustNotBeEmpty(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("TextOutputFormatter_SupportedEncodingsMustNotBeEmpty", (string) null), p0);
//     }
//
//     internal static string TextOutputFormatter_WriteResponseBodyAsyncNotSupported
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (TextOutputFormatter_WriteResponseBodyAsyncNotSupported), (string) null);
//       }
//     }
//
//     internal static string FormatTextOutputFormatter_WriteResponseBodyAsyncNotSupported(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("TextOutputFormatter_WriteResponseBodyAsyncNotSupported", (string) null), p0, p1, p2);
//     }
//
//     internal static string Formatter_NoMediaTypes
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (Formatter_NoMediaTypes), (string) null);
//       }
//     }
//
//     internal static string FormatFormatter_NoMediaTypes(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("Formatter_NoMediaTypes", (string) null), p0, p1);
//     }
//
//     internal static string CouldNotCreateIModelBinder
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (CouldNotCreateIModelBinder), (string) null);
//       }
//     }
//
//     internal static string FormatCouldNotCreateIModelBinder(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("CouldNotCreateIModelBinder", (string) null), p0);
//     }
//
//     internal static string InputFormattersAreRequired
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (InputFormattersAreRequired), (string) null);
//       }
//     }
//
//     internal static string FormatInputFormattersAreRequired(object p0, object p1, object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("InputFormattersAreRequired", (string) null), p0, p1, p2);
//     }
//
//     internal static string ModelBinderProvidersAreRequired
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinderProvidersAreRequired), (string) null);
//       }
//     }
//
//     internal static string FormatModelBinderProvidersAreRequired(object p0, object p1, object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelBinderProvidersAreRequired", (string) null), p0, p1, p2);
//     }
//
//     internal static string OutputFormattersAreRequired
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (OutputFormattersAreRequired), (string) null);
//       }
//     }
//
//     internal static string FormatOutputFormattersAreRequired(object p0, object p1, object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("OutputFormattersAreRequired", (string) null), p0, p1, p2);
//     }
//
//     internal static string MiddewareFilter_ConfigureMethodOverload
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (MiddewareFilter_ConfigureMethodOverload), (string) null);
//       }
//     }
//
//     internal static string FormatMiddewareFilter_ConfigureMethodOverload(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("MiddewareFilter_ConfigureMethodOverload", (string) null), p0);
//     }
//
//     internal static string MiddewareFilter_NoConfigureMethod
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (MiddewareFilter_NoConfigureMethod), (string) null);
//       }
//     }
//
//     internal static string FormatMiddewareFilter_NoConfigureMethod(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("MiddewareFilter_NoConfigureMethod", (string) null), p0, p1);
//     }
//
//     internal static string MiddlewareFilterBuilder_NoMiddlewareFeature
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (MiddlewareFilterBuilder_NoMiddlewareFeature), (string) null);
//       }
//     }
//
//     internal static string FormatMiddlewareFilterBuilder_NoMiddlewareFeature(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("MiddlewareFilterBuilder_NoMiddlewareFeature", (string) null), p0);
//     }
//
//     internal static string MiddlewareFilterBuilder_NullApplicationBuilder
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (MiddlewareFilterBuilder_NullApplicationBuilder), (string) null);
//       }
//     }
//
//     internal static string FormatMiddlewareFilterBuilder_NullApplicationBuilder(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("MiddlewareFilterBuilder_NullApplicationBuilder", (string) null), p0);
//     }
//
//     internal static string MiddlewareFilter_InvalidConfigureReturnType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (MiddlewareFilter_InvalidConfigureReturnType), (string) null);
//       }
//     }
//
//     internal static string FormatMiddlewareFilter_InvalidConfigureReturnType(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("MiddlewareFilter_InvalidConfigureReturnType", (string) null), p0, p1, p2);
//     }
//
//     internal static string MiddlewareFilter_ServiceResolutionFail
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (MiddlewareFilter_ServiceResolutionFail), (string) null);
//       }
//     }
//
//     internal static string FormatMiddlewareFilter_ServiceResolutionFail(
//       object p0,
//       object p1,
//       object p2,
//       object p3)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("MiddlewareFilter_ServiceResolutionFail", (string) null), p0, p1, p2, p3);
//     }
//
//     internal static string AuthorizeFilter_AuthorizationPolicyCannotBeCreated
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (AuthorizeFilter_AuthorizationPolicyCannotBeCreated), (string) null);
//       }
//     }
//
//     internal static string FormatAuthorizeFilter_AuthorizationPolicyCannotBeCreated(
//       object p0,
//       object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("AuthorizeFilter_AuthorizationPolicyCannotBeCreated", (string) null), p0, p1);
//     }
//
//     internal static string FormCollectionModelBinder_CannotBindToFormCollection
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (FormCollectionModelBinder_CannotBindToFormCollection), (string) null);
//       }
//     }
//
//     internal static string FormatFormCollectionModelBinder_CannotBindToFormCollection(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("FormCollectionModelBinder_CannotBindToFormCollection", (string) null), p0, p1, p2);
//     }
//
//     internal static string VaryByQueryKeys_Requires_ResponseCachingMiddleware
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (VaryByQueryKeys_Requires_ResponseCachingMiddleware), (string) null);
//       }
//     }
//
//     internal static string FormatVaryByQueryKeys_Requires_ResponseCachingMiddleware(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("VaryByQueryKeys_Requires_ResponseCachingMiddleware", (string) null), p0);
//     }
//
//     internal static string CandidateResolver_DifferentCasedReference
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (CandidateResolver_DifferentCasedReference), (string) null);
//       }
//     }
//
//     internal static string FormatCandidateResolver_DifferentCasedReference(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("CandidateResolver_DifferentCasedReference", (string) null), p0);
//     }
//
//     internal static string MiddlewareFilterConfigurationProvider_CreateConfigureDelegate_CannotCreateType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (MiddlewareFilterConfigurationProvider_CreateConfigureDelegate_CannotCreateType), (string) null);
//       }
//     }
//
//     internal static string FormatMiddlewareFilterConfigurationProvider_CreateConfigureDelegate_CannotCreateType(
//       object p0,
//       object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("MiddlewareFilterConfigurationProvider_CreateConfigureDelegate_CannotCreateType", (string) null), p0, p1);
//     }
//
//     internal static string Argument_InvalidOffsetLength
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (Argument_InvalidOffsetLength), (string) null);
//       }
//     }
//
//     internal static string FormatArgument_InvalidOffsetLength(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("Argument_InvalidOffsetLength", (string) null), p0, p1);
//     }
//
//     internal static string ComplexTypeModelBinder_NoParameterlessConstructor_ForType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ComplexTypeModelBinder_NoParameterlessConstructor_ForType), (string) null);
//       }
//     }
//
//     internal static string FormatComplexTypeModelBinder_NoParameterlessConstructor_ForType(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ComplexTypeModelBinder_NoParameterlessConstructor_ForType", (string) null), p0);
//     }
//
//     internal static string ComplexTypeModelBinder_NoParameterlessConstructor_ForProperty
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ComplexTypeModelBinder_NoParameterlessConstructor_ForProperty), (string) null);
//       }
//     }
//
//     internal static string FormatComplexTypeModelBinder_NoParameterlessConstructor_ForProperty(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ComplexTypeModelBinder_NoParameterlessConstructor_ForProperty", (string) null), p0, p1, p2);
//     }
//
//     internal static string NoRoutesMatchedForPage
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (NoRoutesMatchedForPage), (string) null);
//       }
//     }
//
//     internal static string FormatNoRoutesMatchedForPage(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("NoRoutesMatchedForPage", (string) null), p0);
//     }
//
//     internal static string UrlHelper_RelativePagePathIsNotSupported
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (UrlHelper_RelativePagePathIsNotSupported), (string) null);
//       }
//     }
//
//     internal static string FormatUrlHelper_RelativePagePathIsNotSupported(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("UrlHelper_RelativePagePathIsNotSupported", (string) null), p0, p1, p2);
//     }
//
//     internal static string ValidationProblemDescription_Title
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ValidationProblemDescription_Title), (string) null);
//       }
//     }
//
//     internal static string ApiController_AttributeRouteRequired
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiController_AttributeRouteRequired), (string) null);
//       }
//     }
//
//     internal static string FormatApiController_AttributeRouteRequired(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApiController_AttributeRouteRequired", (string) null), p0, p1);
//     }
//
//     internal static string VirtualFileResultExecutor_NoFileProviderConfigured
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (VirtualFileResultExecutor_NoFileProviderConfigured), (string) null);
//       }
//     }
//
//     internal static string ApplicationPartFactory_InvalidFactoryType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApplicationPartFactory_InvalidFactoryType), (string) null);
//       }
//     }
//
//     internal static string FormatApplicationPartFactory_InvalidFactoryType(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApplicationPartFactory_InvalidFactoryType", (string) null), p0, p1, p2);
//     }
//
//     internal static string RelatedAssemblyAttribute_AssemblyCannotReferenceSelf
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (RelatedAssemblyAttribute_AssemblyCannotReferenceSelf), (string) null);
//       }
//     }
//
//     internal static string FormatRelatedAssemblyAttribute_AssemblyCannotReferenceSelf(
//       object p0,
//       object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("RelatedAssemblyAttribute_AssemblyCannotReferenceSelf", (string) null), p0, p1);
//     }
//
//     internal static string RelatedAssemblyAttribute_CouldNotBeFound
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (RelatedAssemblyAttribute_CouldNotBeFound), (string) null);
//       }
//     }
//
//     internal static string FormatRelatedAssemblyAttribute_CouldNotBeFound(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("RelatedAssemblyAttribute_CouldNotBeFound", (string) null), p0, p1, p2);
//     }
//
//     internal static string ApplicationAssembliesProvider_DuplicateRelatedAssembly
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApplicationAssembliesProvider_DuplicateRelatedAssembly), (string) null);
//       }
//     }
//
//     internal static string FormatApplicationAssembliesProvider_DuplicateRelatedAssembly(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApplicationAssembliesProvider_DuplicateRelatedAssembly", (string) null), p0);
//     }
//
//     internal static string ApplicationAssembliesProvider_RelatedAssemblyCannotDefineAdditional
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApplicationAssembliesProvider_RelatedAssemblyCannotDefineAdditional), (string) null);
//       }
//     }
//
//     internal static string FormatApplicationAssembliesProvider_RelatedAssemblyCannotDefineAdditional(
//       object p0,
//       object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApplicationAssembliesProvider_RelatedAssemblyCannotDefineAdditional", (string) null), p0, p1);
//     }
//
//     internal static string ComplexTypeModelBinder_NoParameterlessConstructor_ForParameter
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ComplexTypeModelBinder_NoParameterlessConstructor_ForParameter), (string) null);
//       }
//     }
//
//     internal static string FormatComplexTypeModelBinder_NoParameterlessConstructor_ForParameter(
//       object p0,
//       object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ComplexTypeModelBinder_NoParameterlessConstructor_ForParameter", (string) null), p0, p1);
//     }
//
//     internal static string ApiController_MultipleBodyParametersFound
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiController_MultipleBodyParametersFound), (string) null);
//       }
//     }
//
//     internal static string FormatApiController_MultipleBodyParametersFound(
//       object p0,
//       object p1,
//       object p2,
//       object p3)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApiController_MultipleBodyParametersFound", (string) null), p0, p1, p2, p3);
//     }
//
//     internal static string ApiConventionMustBeStatic
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventionMustBeStatic), (string) null);
//       }
//     }
//
//     internal static string FormatApiConventionMustBeStatic(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApiConventionMustBeStatic", (string) null), p0);
//     }
//
//     internal static string InvalidTypeTForActionResultOfT
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (InvalidTypeTForActionResultOfT), (string) null);
//       }
//     }
//
//     internal static string FormatInvalidTypeTForActionResultOfT(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("InvalidTypeTForActionResultOfT", (string) null), p0, p1);
//     }
//
//     internal static string ApiConvention_UnsupportedAttributesOnConvention
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConvention_UnsupportedAttributesOnConvention), (string) null);
//       }
//     }
//
//     internal static string FormatApiConvention_UnsupportedAttributesOnConvention(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApiConvention_UnsupportedAttributesOnConvention", (string) null), p0, p1, p2);
//     }
//
//     internal static string ApiConventionMethod_AmbiguousMethodName
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventionMethod_AmbiguousMethodName), (string) null);
//       }
//     }
//
//     internal static string FormatApiConventionMethod_AmbiguousMethodName(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApiConventionMethod_AmbiguousMethodName", (string) null), p0, p1);
//     }
//
//     internal static string ApiConventionMethod_NoMethodFound
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventionMethod_NoMethodFound), (string) null);
//       }
//     }
//
//     internal static string FormatApiConventionMethod_NoMethodFound(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ApiConventionMethod_NoMethodFound", (string) null), p0, p1);
//     }
//
//     internal static string ValidationVisitor_ExceededMaxDepth
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ValidationVisitor_ExceededMaxDepth), (string) null);
//       }
//     }
//
//     internal static string FormatValidationVisitor_ExceededMaxDepth(
//       object p0,
//       object p1,
//       object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ValidationVisitor_ExceededMaxDepth", (string) null), p0, p1, p2);
//     }
//
//     internal static string ValidationVisitor_ExceededMaxDepthFix
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ValidationVisitor_ExceededMaxDepthFix), (string) null);
//       }
//     }
//
//     internal static string FormatValidationVisitor_ExceededMaxDepthFix(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ValidationVisitor_ExceededMaxDepthFix", (string) null), p0, p1);
//     }
//
//     internal static string ValidationVisitor_ExceededMaxPropertyDepth
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ValidationVisitor_ExceededMaxPropertyDepth), (string) null);
//       }
//     }
//
//     internal static string FormatValidationVisitor_ExceededMaxPropertyDepth(
//       object p0,
//       object p1,
//       object p2,
//       object p3)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ValidationVisitor_ExceededMaxPropertyDepth", (string) null), p0, p1, p2, p3);
//     }
//
//     internal static string ApiConventions_Title_400
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_400), (string) null);
//       }
//     }
//
//     internal static string ApiConventions_Title_401
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_401), (string) null);
//       }
//     }
//
//     internal static string ApiConventions_Title_403
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_403), (string) null);
//       }
//     }
//
//     internal static string ApiConventions_Title_404
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_404), (string) null);
//       }
//     }
//
//     internal static string ApiConventions_Title_406
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_406), (string) null);
//       }
//     }
//
//     internal static string ApiConventions_Title_409
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_409), (string) null);
//       }
//     }
//
//     internal static string ApiConventions_Title_415
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_415), (string) null);
//       }
//     }
//
//     internal static string ApiConventions_Title_422
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_422), (string) null);
//       }
//     }
//
//     internal static string ReferenceToNewtonsoftJsonRequired
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ReferenceToNewtonsoftJsonRequired), (string) null);
//       }
//     }
//
//     internal static string FormatReferenceToNewtonsoftJsonRequired(
//       object p0,
//       object p1,
//       object p2,
//       object p3,
//       object p4)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ReferenceToNewtonsoftJsonRequired", (string) null), p0, p1, p2, p3, p4);
//     }
//
//     internal static string ModelBinding_ExceededMaxModelBindingCollectionSize
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinding_ExceededMaxModelBindingCollectionSize), (string) null);
//       }
//     }
//
//     internal static string FormatModelBinding_ExceededMaxModelBindingCollectionSize(
//       object p0,
//       object p1,
//       object p2,
//       object p3,
//       object p4)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelBinding_ExceededMaxModelBindingCollectionSize", (string) null), p0, p1, p2, p3, p4);
//     }
//
//     internal static string ModelBinding_ExceededMaxModelBindingRecursionDepth
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ModelBinding_ExceededMaxModelBindingRecursionDepth), (string) null);
//       }
//     }
//
//     internal static string FormatModelBinding_ExceededMaxModelBindingRecursionDepth(
//       object p0,
//       object p1,
//       object p2,
//       object p3)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ModelBinding_ExceededMaxModelBindingRecursionDepth", (string) null), p0, p1, p2, p3);
//     }
//
//     internal static string Property_MustBeInstanceOfType
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (Property_MustBeInstanceOfType), (string) null);
//       }
//     }
//
//     internal static string FormatProperty_MustBeInstanceOfType(object p0, object p1, object p2)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("Property_MustBeInstanceOfType", (string) null), p0, p1, p2);
//     }
//
//     internal static string ObjectResultExecutor_MaxEnumerationExceeded
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ObjectResultExecutor_MaxEnumerationExceeded), (string) null);
//       }
//     }
//
//     internal static string FormatObjectResultExecutor_MaxEnumerationExceeded(object p0, object p1)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("ObjectResultExecutor_MaxEnumerationExceeded", (string) null), p0, p1);
//     }
//
//     internal static string UnexpectedJsonEnd
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (UnexpectedJsonEnd), (string) null);
//       }
//     }
//
//     internal static string ApiConventions_Title_500
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (ApiConventions_Title_500), (string) null);
//       }
//     }
//
//     internal static string FailedToReadRequestForm
//     {
//       get
//       {
//         return ResourcesInternal.GetResourceString(nameof (FailedToReadRequestForm), (string) null);
//       }
//     }
//
//     internal static string FormatFailedToReadRequestForm(object p0)
//     {
//       return string.Format((IFormatProvider) ResourcesInternal.Culture, ResourcesInternal.GetResourceString("FailedToReadRequestForm", (string) null), p0);
//     }
//   }
// }
