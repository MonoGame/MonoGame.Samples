#region File Description
//-----------------------------------------------------------------------------
// NormalMappingModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Framework.Content.Pipeline.Builder;
using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace NormalMappingProcessor;

/// <summary>
/// The NormalMappingModelProcessor is used to change the material/effect applied
/// to a model. After going through this processor, the output model will be set
/// up to be rendered with NormalMapping.fx.
/// </summary>
[ContentProcessor(DisplayName = "Model - AscentGame Normal Mapping")]
public class NormalMappingModelProcessor : ModelProcessor
{
    public const string TextureMapKey = "Texture";
    public const string NormalMapKey = "Bump0";
    public const string SpecularMapKey = "Specular0";
    public const string GlowMapKey = "Emissive0";

    static string[] fileKeys = { "Bump0", "Specular0", "Emissive0" };
    static string[] fileExt = { "_n.tga", "_s.tga", "_g.tga" };

    static string ProjectDirectory = PathHelper.Normalize(Directory.GetCurrentDirectory());

    public override ModelContent Process(NodeContent input,
        ContentProcessorContext context)
    {
        if (input == null)
        {
            throw new ArgumentNullException("input");
        }
        context.Logger.LogImportantMessage("processing: " + input.Name);
        PreprocessSceneHierarchy(input, context, input.Name);
        return base.Process(input, context);
    }


    /// <summary>
    /// Recursively calls MeshHelper.CalculateTangentFrames for every MeshContent
    /// object in the NodeContent scene. This function could be changed to add 
    /// more per vertex data as needed.
    /// </summary>
    /// <param initialFileName="input">A node in the scene.  The function should be called
    /// with the root of the scene.</param>
    private void PreprocessSceneHierarchy(NodeContent input,
        ContentProcessorContext context, string inputName)
    {
        MeshContent mesh = input as MeshContent;
        if (mesh != null)
        {
            MeshHelper.CalculateTangentFrames(mesh,
                VertexChannelNames.TextureCoordinate(0),
                VertexChannelNames.Tangent(0),
                VertexChannelNames.Binormal(0));

            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Material.Textures.ContainsKey(TextureMapKey))
                {
                    geometry.Material.Textures.Add(TextureMapKey,
                            new ExternalReference<TextureContent>(
                                    Path.Combine(ProjectDirectory, "null_color.tga")));
                }

                context.Logger.LogImportantMessage("Color map: " + geometry.Material.Textures[TextureMapKey].Filename);

                string colorMap = geometry.Material.Textures[TextureMapKey].Filename;
                string replace = "_c.";
                if (!colorMap.Contains(replace))
                    replace = ".";

                // Normal Map
                if (!geometry.Material.Textures.ContainsKey(NormalMapKey))
                {
                    var normalMap = colorMap.Replace(replace, "_n.");
                    if (!Path.Exists(normalMap))
                        normalMap = Path.Combine(ProjectDirectory, "null_normal.tga");

                    geometry.Material.Textures.Add(NormalMapKey, new ExternalReference<TextureContent>(normalMap));
                }
                context.Logger.LogImportantMessage("Normal map: " + geometry.Material.Textures[NormalMapKey].Filename);

                // Specular map
                if (!geometry.Material.Textures.ContainsKey(SpecularMapKey))
                {
                    var specularMap = colorMap.Replace(replace, "_s.");
                    if (!Path.Exists(specularMap))
                        specularMap = Path.Combine(ProjectDirectory, "null_specular.tga");

                    geometry.Material.Textures.Add(SpecularMapKey, new ExternalReference<TextureContent>(specularMap));
                }
                context.Logger.LogImportantMessage("Specular map: " + geometry.Material.Textures[SpecularMapKey].Filename);

                // Glow map
                if (!geometry.Material.Textures.ContainsKey(GlowMapKey))
                {
                    var glowMap = colorMap.Replace(replace, "_g.");
                    if (!Path.Exists(glowMap))
                        glowMap = Path.Combine(ProjectDirectory, "null_glow.tga");

                    geometry.Material.Textures.Add(GlowMapKey, new ExternalReference<TextureContent>(glowMap));
                }
                context.Logger.LogImportantMessage("Glow map: " + geometry.Material.Textures[GlowMapKey].Filename);
            }
        }

        foreach (NodeContent child in input.Children)
        {
            PreprocessSceneHierarchy(child, context, inputName);
        }
    }

    // acceptableVertexChannelNames are the inputs that the normal map effect
    // expects.  The NormalMappingModelProcessor overrides ProcessVertexChannel
    // to remove all vertex channels which don't have one of these four
    // names.
    static IList<string> acceptableVertexChannelNames =
        new string[]
        {
            VertexChannelNames.TextureCoordinate(0),
            VertexChannelNames.Normal(0),
            VertexChannelNames.Binormal(0),
            VertexChannelNames.Tangent(0)
        };


    /// <summary>
    /// As an optimization, ProcessVertexChannel is overriden to remove data which
    /// is not used by the vertex shader.
    /// </summary>
    /// <param initialFileName="geometry">the geometry object which contains the 
    /// vertex channel</param>
    /// <param initialFileName="vertexChannelIndex">the index of the vertex channel
    /// to operate on</param>
    /// <param initialFileName="context">the context that the processor is operating
    /// under.  in most cases, this parameter isn't necessary; but could
    /// be used to log a warning that a channel had been removed.</param>
    protected override void ProcessVertexChannel(GeometryContent geometry,
        int vertexChannelIndex, ContentProcessorContext context)
    {
        String vertexChannelName =
            geometry.Vertices.Channels[vertexChannelIndex].Name;

        // if this vertex channel has an acceptable names, process it as normal.
        if (acceptableVertexChannelNames.Contains(vertexChannelName))
        {
            base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
        }
        // otherwise, remove it from the vertex channels; it's just extra data
        // we don't need.
        else
        {
            geometry.Vertices.Channels.Remove(vertexChannelName);
        }
    }

    protected override MaterialContent ConvertMaterial(MaterialContent material,
        ContentProcessorContext context)
    {
        EffectMaterialContent normalMappingMaterial = new EffectMaterialContent();
        normalMappingMaterial.Effect = new ExternalReference<EffectContent>
            (Path.Combine(ProjectDirectory, "shaders/NormalMapping.fx"));

        // copy the textures in the original material to the new normal mapping
        // material. this way the diffuse texture is preserved. The
        // PreprocessSceneHierarchy function has already added the normal map
        // texture to the Textures collection, so that will be copied as well.
        foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture
            in material.Textures)
        {
            normalMappingMaterial.Textures.Add(texture.Key, texture.Value);
        }

        // Pass thru texture processing settings.
        OpaqueDataDictionary opaqueDataDictionary = new OpaqueDataDictionary();
        opaqueDataDictionary.Add("ColorKeyColor", ColorKeyColor);
        opaqueDataDictionary.Add("ColorKeyEnabled", ColorKeyEnabled);
        opaqueDataDictionary.Add("GenerateMipmaps", GenerateMipmaps);
        opaqueDataDictionary.Add("PremultiplyTextureAlpha", PremultiplyTextureAlpha);
        opaqueDataDictionary.Add("ResizeTexturesToPowerOfTwo", ResizeTexturesToPowerOfTwo);
        opaqueDataDictionary.Add("TextureFormat", TextureFormat);
        opaqueDataDictionary.Add("DefaultEffect", DefaultEffect);

        return context.Convert<MaterialContent, MaterialContent>
            (normalMappingMaterial, typeof(MaterialProcessor).Name, opaqueDataDictionary);
    }
}