using System;
using System.Reflection;

namespace UnityUtilities.Terrain
{
    [Serializable]
    public class GenerationParameters
    {
        public float baseFrequency = 0.008f;

        public void CopyFrom(GenerationParameters other)
        {
            foreach (
                FieldInfo field in typeof(GenerationParameters).GetFields(
                    BindingFlags.Public | BindingFlags.Instance
                )
            )
            {
                field.SetValue(this, field.GetValue(other));
            }
        }

        public bool IsEqualTo(GenerationParameters other)
        {
            if (other == null)
            {
                return false;
            }

            foreach (
                FieldInfo field in typeof(GenerationParameters).GetFields(
                    BindingFlags.Public | BindingFlags.Instance
                )
            )
            {
                object value1 = field.GetValue(this);
                object value2 = field.GetValue(other);

                if (!Equals(value1, value2))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
