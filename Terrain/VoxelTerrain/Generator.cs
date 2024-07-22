namespace UnityUtilities.Terran
{
    public static class Generator
    {
        public static bool[,,] GetDensityMap((int x, int z) chunkIndex, (int w, int h) chunkSize)
        {
            // Density map is larger than chunk (has 1 unit border around XZ)
            // as neighbor info is needed to build mesh.
            bool[,,] densityMap = new bool[chunkSize.w + 2, chunkSize.h, chunkSize.w + 2];

            for (int x = 0; x < densityMap.GetLength(0); x++)
            {
                for (int z = 0; z < densityMap.GetLength(2); z++)
                {
                    int groundY = (int)(
                        PerlinNoise.Get(
                            (chunkIndex.x * chunkSize.w) + x,
                            (chunkIndex.z * chunkSize.w) + z,
                            seed: null,
                            baseFrequency: 0.001f,
                            numberOfOctaves: 3,
                            lacunarity: 4,
                            persistence: 0.8f
                        ) * chunkSize.h
                    );

                    // const int groundY = 128;

                    for (int y = 0; y < densityMap.GetLength(1); y++)
                    {
                        densityMap[x, y, z] = y <= groundY;
                    }
                }
            }

            return densityMap;
        }
    }
}
