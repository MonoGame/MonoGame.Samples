using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Content;
namespace MonoGameLibrary.Graphics;

public class Material
{
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

    public Material(WatchedAsset<Effect> asset)
    {
        Asset = asset;
        UpdateParameterCache();
    }
    
    public void SetParameter(string name, float value)
    {
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
}