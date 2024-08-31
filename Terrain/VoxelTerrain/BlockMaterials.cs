using System;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    [Serializable]
    public struct BlockMaterials
    {
        public Material[] materials;

        [SerializeField]
        private Configuration configuration;

        public BlockMaterials(Material allSides)
        {
            this.configuration = Configuration.SingleMaterial;
            this.materials = new[] { allSides };
        }

        public BlockMaterials(Material top, Material sides, Material bottom)
        {
            this.configuration = Configuration.TopSidesBottom;
            this.materials = new[] { top, sides, bottom };
        }

        private enum Configuration : byte
        {
            SingleMaterial,
            TopSidesBottom,
        }

        public readonly Material GetMaterial(Face face)
        {
            return this.materials[this.GetMaterialIndex(face)];
        }

        public readonly byte GetMaterialIndex(Face face)
        {
            if (this.configuration == Configuration.SingleMaterial)
            {
                Assert.That(this.materials.Length == 1);

                return 0;
            }

            if (this.configuration == Configuration.TopSidesBottom)
            {
                Assert.That(this.materials.Length == 3);

                if (face == Face.YPlus)
                {
                    return 0;
                }

                if (face == Face.YMinus)
                {
                    return 2;
                }

                return 1;
            }

            throw new Exception("Unreachable code.");
        }
    }
}
