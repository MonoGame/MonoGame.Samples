using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class TextureAtlas
{
    private Dictionary<string, TextureRegion> _regions;

    /// <summary>
    /// Gets or Sets the source texture represented by this texture atlas.
    /// </summary>
    public Texture2D Texture { get; set; }

    /// <summary>
    /// Creates a new texture atlas.
    /// </summary>
    public TextureAtlas()
    {
        _regions = new Dictionary<string, TextureRegion>();
    }

    /// <summary>
    /// Creates a new texture atlas instance using the given texture.
    /// </summary>
    /// <param name="texture">The source texture represented by the texture atlas.</param>
    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
        _regions = new Dictionary<string, TextureRegion>();
    }

    /// <summary>
    /// Creates a new region and adds it to this texture atlas.
    /// </summary>
    /// <param name="name">The name to give the texture region.</param>
    /// <param name="x">The top-left x-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
    /// <param name="y">The top-left y-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
    /// <param name="width">The width, in pixels, of the region.</param>
    /// <param name="height">The height, in pixels, of the region.</param>
    public void AddRegion(string name, int x, int y, int width, int height)
    {
        TextureRegion region = new TextureRegion(Texture, x, y, width, height);
        _regions.Add(name, region);
    }

    /// <summary>
    /// Gets the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to retrieve.</param>
    /// <returns>The TextureRegion with the specified name.</returns>
    public TextureRegion GetRegion(string name)
    {
        return _regions[name];
    }

    /// <summary>
    /// Removes the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to remove.</param>
    /// <returns></returns>
    public bool RemoveRegion(string name)
    {
        return _regions.Remove(name);
    }

    /// <summary>
    /// Removes all regions from this texture atlas.
    /// </summary>
    public void Clear()
    {
        _regions.Clear();
    }

    /// <summary>
    /// Creates a new texture atlas based a texture atlas xml configuration file.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the atlas.</param>
    /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
    /// <returns>The texture atlas created by this method.</returns>
    public static TextureAtlas FromFile(ContentManager content, string fileName)
    {
        TextureAtlas atlas = new TextureAtlas();

        string filePath = Path.Combine(content.RootDirectory, fileName);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XDocument doc = XDocument.Load(reader);
                XElement root = doc.Root;

                // The <Texture> element contains the content path for the Texture2D to load.
                // So we will retrieve that value then use the content manager to load the texture.
                string texturePath = root.Element("Texture").Value;
                atlas.Texture = content.Load<Texture2D>(texturePath);

                // The <Regions> element contains individual <Region> elements, each one describing
                // a different texture region within the atlas.
                //
                // Example:
                // <Regions>
                //      <Region name="spriteOne" x="0" y="0" width="32" height="32" />
                //      <Region name="spriteTwo" x="32" y="0" width="32" height="32" />
                // </Regions>
                //
                // So we retrieve all of the <Region> elements then loop through each one
                // and generate a new TextureRegion instance from it and add it to this atlas.
                var regions = root.Element("Regions")?.Elements("Region");

                if (regions != null)
                {
                    foreach (var region in regions)
                    {
                        string name = region.Attribute("name")?.Value;
                        int x = int.Parse(region.Attribute("x")?.Value ?? "0");
                        int y = int.Parse(region.Attribute("y")?.Value ?? "0");
                        int width = int.Parse(region.Attribute("width")?.Value ?? "0");
                        int height = int.Parse(region.Attribute("height")?.Value ?? "0");

                        if (!string.IsNullOrEmpty(name))
                        {
                            atlas.AddRegion(name, x, y, width, height);
                        }
                    }
                }

                return atlas;
            }
        }
    }
}
