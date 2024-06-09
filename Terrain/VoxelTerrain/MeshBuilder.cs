using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    public static class MeshBuilder
    {
        public static Mesh BuildChunkMesh(
            (int x, int z) chunkIndex,
            (int w, int h) chunkSize,
            Texture blockTextureAtlas,
            int blockTextureSize
        )
        {
            string name = FormattableString.Invariant($"Chunk_x{chunkIndex.x}_z{chunkIndex.z}");

            bool[,,] densityMap = GetDensityMap(chunkIndex, chunkSize);

            var ts = new List<int>(); // Triangles
            var vs = new List<Vector3>(); // Vertices
            var uvs = new List<Vector2>();

            int chunkHeight = densityMap.GetLength(1);

            void AddFace(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
            {
                int v = vs.Count;

                vs.Add(v0);
                vs.Add(v1);
                vs.Add(v2);
                vs.Add(v3);

                ts.Add(v);
                ts.Add(v + 1);
                ts.Add(v + 2);

                ts.Add(v);
                ts.Add(v + 2);
                ts.Add(v + 3);

                uvs.AddRange(
                    GetUVs(
                        0, // TODO: Remove hardcoded block index.
                        (blockTextureAtlas.width, blockTextureAtlas.height),
                        blockTextureSize
                    )
                );
            }

            // Note x and z start at 1 and end 1 iteration earlier due to 1-unit
            // border in density map.
            for (int x = 1; x < densityMap.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < densityMap.GetLength(1); y++)
                {
                    for (int z = 1; z < densityMap.GetLength(2) - 1; z++)
                    {
                        if (!densityMap[x, y, z])
                        {
                            continue;
                        }

                        // Offset to compensate 1-unit border.
                        var block = new Vector3Int(x - 1, y, z - 1);

                        // X+
                        if (!densityMap[x + 1, y, z])
                        {
                            AddFace(
                                block + new Vector3(1, 0, 0),
                                block + new Vector3(1, 1, 0),
                                block + new Vector3(1, 1, 1),
                                block + new Vector3(1, 0, 1)
                            );
                        }

                        // X-
                        if (!densityMap[x - 1, y, z])
                        {
                            AddFace(
                                block + new Vector3(0, 0, 1),
                                block + new Vector3(0, 1, 1),
                                block + new Vector3(0, 1, 0),
                                block + new Vector3(0, 0, 0)
                            );
                        }

                        // Y+
                        if (y < chunkHeight - 1 && !densityMap[x, y + 1, z])
                        {
                            AddFace(
                                block + new Vector3(0, 1, 0),
                                block + new Vector3(0, 1, 1),
                                block + new Vector3(1, 1, 1),
                                block + new Vector3(1, 1, 0)
                            );
                        }

                        // Y-
                        if (y > 0 && !densityMap[x, y - 1, z])
                        {
                            AddFace(
                                block + new Vector3(0, 0, 1),
                                block + new Vector3(0, 0, 0),
                                block + new Vector3(1, 0, 0),
                                block + new Vector3(1, 0, 1)
                            );
                        }

                        // Z+
                        if (!densityMap[x, y, z + 1])
                        {
                            AddFace(
                                block + new Vector3(1, 0, 1),
                                block + new Vector3(1, 1, 1),
                                block + new Vector3(0, 1, 1),
                                block + new Vector3(0, 0, 1)
                            );
                        }

                        // Z-
                        if (!densityMap[x, y, z - 1])
                        {
                            AddFace(
                                block + new Vector3(0, 0, 0),
                                block + new Vector3(0, 1, 0),
                                block + new Vector3(1, 1, 0),
                                block + new Vector3(1, 0, 0)
                            );
                        }
                    }
                }
            }

            var mesh = new Mesh()
            {
                name = name,
                vertices = vs.ToArray(),
                triangles = ts.ToArray(),
                uv = uvs.ToArray(),
            };

            // Do not optimize as meshes are created at run time.
            // mesh.Optimize();

            return mesh;
        }

        private static bool[,,] GetDensityMap((int x, int z) chunkIndex, (int w, int h) chunkSize)
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
                            (chunkIndex.z * chunkSize.w) + z
                        ) * chunkSize.h
                    );

                    for (int y = 0; y < densityMap.GetLength(1); y++)
                    {
                        densityMap[x, y, z] = y <= groundY;
                    }
                }
            }

            return densityMap;
        }

        private static Vector2[] GetUVs(
            int blockIndex,
            (int w, int h) textureAtlasSize,
            int blockTextureSize
        )
        {
            int tw = textureAtlasSize.w;
            int th = textureAtlasSize.h;

            int bw = blockTextureSize + 1; // Block width (1px padding).

            int blocksPerRow = (tw - 1) / bw;

            int blockX = blockIndex % blocksPerRow;
            int blockY = blockIndex / blocksPerRow;

            float u = (float)(1 + (blockX * bw)) / tw;
            float v = (float)(th - ((blockY + 1) * bw)) / th;

            float bw01 = (float)bw / tw;
            float bh01 = (float)bw / th;

            return new Vector2[]
            {
                new(u, v),
                new(u, v + bh01),
                new(u + bw01, v + bh01),
                new(u + bw01, v),
            };
        }
    }
}
