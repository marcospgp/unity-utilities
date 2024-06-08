using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UnityUtilities
{
    public class VoxelTerrain : MonoBehaviour
    {
        private const int CHUNK_WIDTH = 16;
        private const int CHUNK_HEIGHT = 256;

        [SerializeField]
        private int viewDistance = 16;

        [SerializeField, LayerSelect]
        private int groundLayer;

        [SerializeField]
        private Material groundMaterial;

        public void Start()
        {
            foreach ((int x, int z) in Spiral(this.viewDistance))
            {
                string name = FormattableString.Invariant($"Chunk_x{x}_z{z}");

                bool[,,] densityMap = GetDensityMap(x, z);

                Mesh chunkMesh = BuildChunkMesh(name, densityMap);

                var chunk = new GameObject { name = name, layer = this.groundLayer };

                chunk.transform.position = new Vector3(x * CHUNK_WIDTH, 0, z * CHUNK_WIDTH);

                MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();

                meshRenderer.material = this.groundMaterial;

                MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();

                // Use sharedMesh and not mesh to avoid making a copy
                // unnecessarily.
                meshFilter.sharedMesh = chunkMesh;
            }
        }

        private static bool[,,] GetDensityMap(int chunkX, int chunkZ)
        {
            // Density map is larger than chunk (has 1 unit border around XZ)
            // as neighbor info is needed to build mesh.
            bool[,,] densityMap = new bool[CHUNK_WIDTH + 2, CHUNK_HEIGHT, CHUNK_WIDTH + 2];

            for (int x = 0; x < densityMap.GetLength(0); x++)
            {
                for (int z = 0; z < densityMap.GetLength(2); z++)
                {
                    int groundY = (int)(
                        PerlinNoise.Get((chunkX * CHUNK_WIDTH) + x, (chunkZ * CHUNK_WIDTH) + z)
                        * CHUNK_HEIGHT
                    );

                    for (int y = 0; y < densityMap.GetLength(1); y++)
                    {
                        densityMap[x, y, z] = y <= groundY;
                    }
                }
            }

            return densityMap;
        }

        private static Mesh BuildChunkMesh(string name, bool[,,] densityMap)
        {
            var ts = new List<int>(); // Triangles
            var vs = new List<Vector3>(); // Vertices

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
                        if (y < CHUNK_HEIGHT - 1 && !densityMap[x, y + 1, z])
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
            };

            // Do not optimize as meshes are created at run time.
            // mesh.Optimize();

            return mesh;
        }

        // Enumerates coordinates for a spiral.
        private static IEnumerable<(int, int)> Spiral(int radius)
        {
            int radiusSquared = radius * radius;
            int x = 0;
            int z = 0;

            yield return (x, z);

            bool IsWithinRadius(int x, int z) => ((x * x) + (z * z)) <= radiusSquared;

            for (int i = 1; i < radius; i++)
            {
                z -= 1;

                for (; x < +i; x++)
                {
                    if (IsWithinRadius(x, z))
                    {
                        yield return (x, z);
                    }
                }

                for (; z < +i; z++)
                {
                    if (IsWithinRadius(x, z))
                    {
                        yield return (x, z);
                    }
                }

                for (; x > -i; x--)
                {
                    if (IsWithinRadius(x, z))
                    {
                        yield return (x, z);
                    }
                }

                for (; z > -i; z--)
                {
                    if (IsWithinRadius(x, z))
                    {
                        yield return (x, z);
                    }
                }

                if (IsWithinRadius(x, z))
                {
                    yield return (x, z);
                }
            }
        }
    }
}
