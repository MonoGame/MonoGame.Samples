using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Content;
namespace MonoGameLibrary.Graphics;

public class Material
{
    // materials that will be drawn during the standard debug UI pass.
    private static HashSet<Material> s_debugMaterials = new HashSet<Material>();

    /// <summary>
    /// The hot-reloadable asset that this material is using
    /// </summary>
    public WatchedAsset<Effect> Asset;
    
    /// <summary>
    /// A cached version of the parameters available in the shader
    /// </summary>
    public Dictionary<string, EffectParameter> ParameterMap;
    
    /// <summary>
    /// The currently loaded Effect that this material is using
    /// </summary>
    public Effect Effect => Asset.Asset;

    /// <summary>
    /// Enable this variable to visualize the debugUI for the material
    /// </summary>
    public bool IsDebugVisible
    {
        get
        {
            return s_debugMaterials.Contains(this);
        }
        set
        {
            if (IsDebugVisible)
            {
                s_debugMaterials.Remove(this);
            }
            else
            {
                s_debugMaterials.Add(this);
            }
        }
    }

    /// <summary>
    /// When true, the debug UI will override parameters
    /// </summary>
    public bool DebugOverride;

    public Material(WatchedAsset<Effect> asset)
    {
        Asset = asset;
        UpdateParameterCache();
    }
    
    public void SetParameter(string name, float value)
    {
        if (DebugOverride) return;

        if (TryGetParameter(name, out var parameter))
        {
            parameter.SetValue(value);
        }
        else
        {
            Console.WriteLine($"Warning: cannot set shader parameter=[{name}] because it does not exist in the compiled shader=[{Asset.AssetName}]");
        }
    }
    
    public void SetParameter(string name, Matrix value)
    {
        if (DebugOverride) return;

        if (TryGetParameter(name, out var parameter))
        {
            parameter.SetValue(value);
        }
        else
        {
            Console.WriteLine($"Warning: cannot set shader parameter=[{name}] because it does not exist in the compiled shader=[{Asset.AssetName}]");
        }
    }
    
    public void SetParameter(string name, Vector2 value)
    {
        if (DebugOverride) return;

        if (TryGetParameter(name, out var parameter))
        {
            parameter.SetValue(value);
        }
        else
        {
            Console.WriteLine($"Warning: cannot set shader parameter=[{name}] because it does not exist in the compiled shader=[{Asset.AssetName}]");
        }
    }

    public void SetParameter(string name, Texture2D value)
    {
        if (DebugOverride) return;

        if (TryGetParameter(name, out var parameter))
        {
            parameter.SetValue(value);
        }
        else
        {
            Console.WriteLine($"Warning: cannot set shader parameter=[{name}] because it does not exist in the compiled shader=[{Asset.AssetName}]");
        }
    }

    /// <summary>
    /// Check if the given parameter name is available in the compiled shader code.
    /// Remember that a parameter will be optimized out of a shader if it is not being used
    /// in the shader's return value.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public bool TryGetParameter(string name, out EffectParameter parameter)
    {
        return ParameterMap.TryGetValue(name, out parameter);
    }
    
    /// <summary>
    /// Rebuild the <see cref="ParameterMap"/> based on the current parameters available in the effect instance
    /// </summary>
    public void UpdateParameterCache()
    {
        ParameterMap = Effect.Parameters.ToDictionary(p => p.Name);
    }
    
    [Conditional("DEBUG")]
    public void Update()
    {
        if (Asset.TryRefresh(out var oldAsset))
        {
            UpdateParameterCache();
            
            foreach (var oldParam in oldAsset.Parameters)
            {
                if (!TryGetParameter(oldParam.Name, out var newParam))
                {
                    continue;
                }
                
                switch (oldParam.ParameterClass)
                {
                    case EffectParameterClass.Scalar:
                        newParam.SetValue(oldParam.GetValueSingle());
                        break;
                    case EffectParameterClass.Matrix:
                        newParam.SetValue(oldParam.GetValueMatrix());
                        break;
                    case EffectParameterClass.Vector when oldParam.ColumnCount == 2: // float2
                        newParam.SetValue(oldParam.GetValueVector2());
                        break;
                    case EffectParameterClass.Object:
                        newParam.SetValue(oldParam.GetValueTexture2D());
                        break;
                    default:
                        Console.WriteLine("Warning: shader reload system was not able to re-apply property. " +
                                          $"shader=[{Effect.Name}] " +
                                          $"property=[{oldParam.Name}] " +
                                          $"class=[{oldParam.ParameterClass}]");
                        break;
                }
            }
        }
    }
    
    
    
    [Conditional("DEBUG")]
    public void DrawDebug()
    {
        ImGui.Begin(Effect.Name);
        
        var currentSize = ImGui.GetWindowSize();
        ImGui.SetWindowSize(Effect.Name, new System.Numerics.Vector2(MathHelper.Max(100, currentSize.X), MathHelper.Max(100, currentSize.Y)));
        
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Last Updated");
        ImGui.SameLine();
        ImGui.LabelText("##last-updated", Asset.UpdatedAt.ToString() + $" ({(DateTimeOffset.Now - Asset.UpdatedAt).ToString(@"h\:mm\:ss")} ago)");
        
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Override Values");
        ImGui.SameLine();
        ImGui.Checkbox("##override-values", ref DebugOverride);
        
        ImGui.NewLine();
        
        bool ScalarSlider(string key, ref float value)
        {
            float min = 0;
            float max = 1;
            
            return ImGui.SliderFloat($"##_prop{key}", ref value, min, max);
        }
        
        foreach (var prop in ParameterMap)
        {
            switch (prop.Value.ParameterType, prop.Value.ParameterClass)
            {
                case (EffectParameterType.Single, EffectParameterClass.Scalar):
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(prop.Key);
                    ImGui.SameLine();
                                
                    var value = prop.Value.GetValueSingle();
                    if (ScalarSlider(prop.Key, ref value))
                    {
                        prop.Value.SetValue(value);
                    }
                    break;
                
                case (EffectParameterType.Single, EffectParameterClass.Vector):
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(prop.Key);

                    var vec2Value = prop.Value.GetValueVector2();
                    ImGui.Indent();
                    
                    ImGui.Text("X");
                    ImGui.SameLine();
                    
                    if (ScalarSlider(prop.Key + ".x", ref vec2Value.X))
                    {
                        prop.Value.SetValue(vec2Value);
                    }
                    
                    ImGui.Text("Y");
                    ImGui.SameLine();
                    if (ScalarSlider(prop.Key + ".y", ref vec2Value.Y))
                    {
                        prop.Value.SetValue(vec2Value);
                    }
                    ImGui.Unindent();
                    break;
                
                case (EffectParameterType.Texture2D, EffectParameterClass.Object):
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(prop.Key);
                    ImGui.SameLine();

                    var texture = prop.Value.GetValueTexture2D();
                    if (texture != null)
                    {
                        var texturePtr = Core.ImGuiRenderer.BindTexture(texture);
                        ImGui.Image(texturePtr, new System.Numerics.Vector2(texture.Width, texture.Height));
                    }
                    else
                    {
                        ImGui.Text("(null)");
                    }
                    break;
                
                default:
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(prop.Key);
                    ImGui.SameLine();
                    ImGui.Text($"(unsupported {prop.Value.ParameterType}, {prop.Value.ParameterClass})");
                    break;
            }
        }
        ImGui.End();
    }

    [Conditional("DEBUG")]
    public static void DrawVisibleDebugUi(GameTime gameTime)
    {
        // first, cull any materials that are not visible, or disposed. 
        var toRemove = new List<Material>();
        foreach (var material in s_debugMaterials)
        {
            if (material.Effect.IsDisposed)
            {
                toRemove.Add(material);
            }
        }

        foreach (var material in toRemove)
        {
            s_debugMaterials.Remove(material);
        }
        
        Core.ImGuiRenderer.BeforeLayout(gameTime);
        foreach (var material in s_debugMaterials)
        {
            material.DrawDebug();
        }
        Core.ImGuiRenderer.AfterLayout();
    }
}