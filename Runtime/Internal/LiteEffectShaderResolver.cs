using System;
using UnityEngine;

namespace Acfeel.UIToolkitLiteEffects
{
    internal static class LiteEffectShaderResolver
    {
        public static Shader Resolve(string resourceName, string shaderName)
        {
            var shader = Resources.Load<Shader>(resourceName);
            if (shader == null)
            {
                shader = Shader.Find(shaderName);
            }

            if (shader == null)
            {
                throw new InvalidOperationException($"{resourceName} shader was not found.");
            }

            return shader;
        }
    }
}
