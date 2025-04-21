namespace JsonApiDotNetCore.Configuration;

internal interface IJsonApiApplicationBuilderEvents
{
    void ResourceGraphBuilt(IResourceGraph resourceGraph);
}
