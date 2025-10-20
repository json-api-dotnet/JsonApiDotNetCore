namespace JsonApiDotNetCore.SourceGenerators;

/// <summary>
/// Supplemental information that is derived from the core analysis, which is expensive to produce.
/// </summary>
internal readonly record struct FullControllerInfo(
    CoreControllerInfo CoreController, TypeInfo ControllerType, TypeInfo LoggerFactoryInterface, string HintFileName)
{
    // Using readonly fields, so they can be passed by reference (using 'in' modifier, to avoid making copies) during code generation.
    public readonly CoreControllerInfo CoreController = CoreController;
    public readonly TypeInfo ControllerType = ControllerType;
    public readonly TypeInfo LoggerFactoryInterface = LoggerFactoryInterface;
    public readonly string HintFileName = HintFileName;

    public static FullControllerInfo Create(CoreControllerInfo coreController, string controllerTypeName)
    {
        var controllerTypeInfo = new TypeInfo(coreController.ControllerNamespace, controllerTypeName);
        var loggerFactoryTypeInfo = new TypeInfo("Microsoft.Extensions.Logging", "ILoggerFactory");

        return new FullControllerInfo(coreController, controllerTypeInfo, loggerFactoryTypeInfo, controllerTypeName);
    }

    public FullControllerInfo WithHintFileName(string hintFileName)
    {
        // ReSharper disable once UseWithExpressionToCopyRecord
        // Justification: Workaround for bug at https://youtrack.jetbrains.com/issue/RSRP-502017/Invalid-suggestion-to-use-with-expression.
        return new FullControllerInfo(CoreController, ControllerType, LoggerFactoryInterface, hintFileName);
    }
}
