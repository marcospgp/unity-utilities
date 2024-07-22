using System;
using System.Collections.Generic;
using UnityEngine;
using UnityUtilities.Terran;

namespace UnityUtilities.Terrain
{
    public static class MeshBuilder
    {
        public static Mesh BuildChunkMesh((int x, int z) chunkIndex, (int w, int h) chunkSize)
        {
            string name = FormattableString.Invariant($"Chunk_x{chunkIndex.x}_z{chunkIndex.z}");

            bool[,,] densityMap = Generator.GetDensityMap(chunkIndex, chunkSize);

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

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(1, 0));
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
    }
}
