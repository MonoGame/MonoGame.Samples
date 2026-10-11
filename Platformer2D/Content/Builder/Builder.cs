/// <summary>
/// Entry point for the Content Builder project, 
/// which when executed will build content according to the "Content Collection Strategy" defined in the Builder class.
/// </summary>
/// <remarks>
/// Make sure to validate the directory paths in the "ContentBuilderParams" for your specific project.
/// For more details regarding the Content Builder, see the MonoGame documentation: <tbc.>
/// </remarks>

using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

var contentCollectionArgs = new ContentBuilderParams()
{
    Mode = ContentBuilderMode.Builder,
    WorkingDirectory = $"{AppContext.BaseDirectory}../../", // path to where your content folder can be located
    SourceDirectory = "Assets", // Not actually needed as this is the default, but added for reference
    Platform = TargetPlatform.DesktopVK
};
var builder = new Builder();

if (args is not null && args.Length > 0)
{
    builder.Run(args);
}
else
{
    builder.Run(contentCollectionArgs);
}

return builder.FailedToBuild > 0 ? -1 : 0;

public class Builder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var contentCollection = new ContentCollection();

        // By default, no content will be imported from the Assets folder using the default importer for their file type.
        // Please define your content collection rules here.

        /* Examples

        // Import all content in the Assets folder using the default importer for their file type.

        // Only copy content from the assets folder rather than build it with the pipeline.
        content.IncludeCopy<WildcardRule>("*.json");

        // Exclude assets that match the pattern., only required overriding a default import behaviour.
        content.Exclude<WildcardRule>("Font/*.txt");

        // Include a specific asset with processor parameters.
        content.Include("Models/character.glb", new FbxImporter(),
            new MeshAnimatedModelProcessor()
            {
                Scale = 100.0f
            }
        );
        */
        contentCollection.Include<WildcardRule>("*");
        contentCollection.IncludeCopy<WildcardRule>("Levels/*.txt");
        contentCollection.Exclude<WildcardRule>("*.ttf");

        return contentCollection;
    }
}