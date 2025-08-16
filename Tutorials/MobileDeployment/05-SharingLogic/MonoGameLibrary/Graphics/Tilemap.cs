using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class Tilemap
{
    private readonly Tileset _tileset;
    private readonly int[] _tiles;

    /// <summary>
    /// Gets the total number of rows in this tilemap.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Gets the total number of columns in this tilemap.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets the total number of tiles in this tilemap.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets or Sets the scale factor to draw each tile at.
    /// </summary>
    public Vector2 Scale { get; set; }

    /// <summary>
    /// Gets the width, in pixels, each tile is drawn at.
    /// </summary>
    public float TileWidth => _tileset.TileWidth * Scale.X;

    /// <summary>
    /// Gets the height, in pixels, each tile is drawn at.
    /// </summary>
    public float TileHeight => _tileset.TileHeight * Scale.Y;

    /// <summary>
    /// Creates a new tilemap.
    /// </summary>
    /// <param name="tileset">The tileset used by this tilemap.</param>
    /// <param name="columns">The total number of columns in this tilemap.</param>
    /// <param name="rows">The total number of rows in this tilemap.</param>
    public Tilemap(Tileset tileset, int columns, int rows)
    {
        _tileset = tileset;
        Rows = rows;
        Columns = columns;
        Count = Columns * Rows;
        Scale = Vector2.One;
        _tiles = new int[Count];
    }

    /// <summary>
    /// Sets the tile at the given index in this tilemap to use the tile from
    /// the tileset at the specified tileset id.
    /// </summary>
    /// <param name="index">The index of the tile in this tilemap.</param>
    /// <param name="tilesetID">The tileset id of the tile from the tileset to use.</param>
    public void SetTile(int index, int tilesetID)
    {
        _tiles[index] = tilesetID;
    }

    /// <summary>
    /// Sets the tile at the given column and row in this tilemap to use the tile
    /// from the tileset at the specified tileset id.
    /// </summary>
    /// <param name="column">The column of the tile in this tilemap.</param>
    /// <param name="row">The row of the tile in this tilemap.</param>
    /// <param name="tilesetID">The tileset id of the tile from the tileset to use.</param>
    public void SetTile(int column, int row, int tilesetID)
    {
        int index = row * Columns + column;
        SetTile(index, tilesetID);
    }

    /// <summary>
    /// Gets the texture region of the tile from this tilemap at the specified index.
    /// </summary>
    /// <param name="index">The index of the tile in this tilemap.</param>
    /// <returns>The texture region of the tile from this tilemap at the specified index.</returns>
    public TextureRegion GetTile(int index)
    {
        return _tileset.GetTile(_tiles[index]);
    }

    /// <summary>
    /// Gets the texture region of the tile frm this tilemap at the specified
    /// column and row.
    /// </summary>
    /// <param name="column">The column of the tile in this tilemap.</param>
    /// <param name="row">The row of hte tile in this tilemap.</param>
    /// <returns>The texture region of the tile from this tilemap at the specified column and row.</returns>
    public TextureRegion GetTile(int column, int row)
    {
        int index = row * Columns + column;
        return GetTile(index);
    }

    /// <summary>
    /// Draws this tilemap using the given sprite batch.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to draw this tilemap.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < Count; i++)
        {
            int tileSetIndex = _tiles[i];
            TextureRegion tile = _tileset.GetTile(tileSetIndex);

            int x = i % Columns;
            int y = i / Columns;

            Vector2 position = new Vector2(x * TileWidth, y * TileHeight);
            tile.Draw(spriteBatch, position, Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
        }
    }

    /// <summary>
    /// Creates a new tilemap based on a tilemap xml configuration file.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the tileset.</param>
    /// <param name="filename">The path to the xml file, relative to the content root directory.</param>
    /// <returns>The tilemap created by this method.</returns>
    public static Tilemap FromFile(ContentManager content, string filename)
    {
        string filePath = Path.Combine(content.RootDirectory, filename);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XDocument doc = XDocument.Load(reader);
                XElement root = doc.Root;

                // The <Tileset> element contains the information about the tileset
                // used by the tilemap.
                //
                // Example
                // <Tileset region="0 0 100 100" tileWidth="10" tileHeight="10">contentPath</Tileset>
                //
                // The region attribute represents the x, y, width, and height
                // components of the boundary for the texture region within the
                // texture at the contentPath specified.
                //
                // the tileWidth and tileHeight attributes specify the width and
                // height of each tile in the tileset.
                //
                // the contentPath value is the contentPath to the texture to
                // load that contains the tileset
                XElement tilesetElement = root.Element("Tileset");

                string regionAttribute = tilesetElement.Attribute("region").Value;
                string[] split = regionAttribute.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                int x = int.Parse(split[0]);
                int y = int.Parse(split[1]);
                int width = int.Parse(split[2]);
                int height = int.Parse(split[3]);

                int tileWidth = int.Parse(tilesetElement.Attribute("tileWidth").Value);
                int tileHeight = int.Parse(tilesetElement.Attribute("tileHeight").Value);
                string contentPath = tilesetElement.Value;

                // Load the texture 2d at the content path
                Texture2D texture = content.Load<Texture2D>(contentPath);

                // Create the texture region from the texture
                TextureRegion textureRegion = new TextureRegion(texture, x, y, width, height);

                // Create the tileset using the texture region
                Tileset tileset = new Tileset(textureRegion, tileWidth, tileHeight);

                // The <Tiles> element contains lines of strings where each line
                // represents a row in the tilemap.  Each line is a space
                // separated string where each element represents a column in that
                // row.  The value of the column is the id of the tile in the
                // tileset to draw for that location.
                //
                // Example:
                // <Tiles>
                //      00 01 01 02
                //      03 04 04 05
                //      03 04 04 05
                //      06 07 07 08
                // </Tiles>
                XElement tilesElement = root.Element("Tiles");

                // Split the value of the tiles data into rows by splitting on
                // the new line character
                string[] rows = tilesElement.Value.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                // Split the value of the first row to determine the total number of columns
                int columnCount = rows[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;

                // Create the tilemap
                Tilemap tilemap = new Tilemap(tileset, columnCount, rows.Length);

                // Process each row
                for (int row = 0; row < rows.Length; row++)
                {
                    // Split the row into individual columns
                    string[] columns = rows[row].Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    // Process each column of the current row
                    for (int column = 0; column < columnCount; column++)
                    {
                        // Get the tileset index for this location
                        int tilesetIndex = int.Parse(columns[column]);

                        // Get the texture region of that tile from the tileset
                        TextureRegion region = tileset.GetTile(tilesetIndex);

                        // Add that region to the tilemap at the row and column location
                        tilemap.SetTile(column, row, tilesetIndex);
                    }
                }

                return tilemap;
            }
        }
    }
}
