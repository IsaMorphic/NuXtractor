/*
 *  Copyright 2020 Chosen Few Software
 *  This file is part of NuXtractor.
 *
 *  NuXtractor is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  NuXtractor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with NuXtractor.  If not, see <https://www.gnu.org/licenses/>.
 */

using NuXtractor.Materials;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor.Models
{
    public abstract class Model
    {
        public Material Material { get; }

        public Model(Material material)
        {
            Material = material;
        }

        public abstract Task<Vertex[]> GetVerticesAsync();
        public abstract Task<Triangle[]> GetTrianglesAsync();
        
        private class StripCandidate
        {
            public int[] Indicies { get; }
            public List<Triangle> Triangles { get; }

            public StripCandidate(int[] indicies, List<Triangle> triangles)
            {
                Indicies = indicies;
                Triangles = triangles;
            }
        }

        public class ConcreteTriangleStrip : TriangleStrip
        {
            private int[] Indicies { get; }
            private Vertex[] Vertices { get; }

            public ConcreteTriangleStrip(Material material, int[] indicies, Vertex[] vertices) : base(material)
            {
                Indicies = indicies;
                Vertices = vertices;
            }

            public override Task<int[]> GetIndiciesAsync()
            {
                return Task.FromResult(Indicies);
            }

            public override Task<Vertex[]> GetVerticesAsync()
            {
                return Task.FromResult(Vertices);
            }
        }

        public virtual async Task<TriangleStrip[]> ToTriangleStripsAsync()
        {
            var triangles = await GetTrianglesAsync();
            var vertices = await GetVerticesAsync();

            List<Triangle> remainingTris = triangles.ToList();
            List<TriangleStrip> strips = new List<TriangleStrip>();

            while (remainingTris.Any())
            {
                // Start with least connected triangle (triangle with least amount of neighbors that aren't part of a strip)
                Triangle startTri = remainingTris
                    .OrderBy(t1 =>
                        remainingTris.Count(
                            t2 => t1.GetEdges().Intersect(t2.GetEdges()).Any()
                            ))
                    .First();

                // Create a strip starting from each edge of seed triangle
                List<StripCandidate> candidates = new List<StripCandidate>();
                foreach (var startEdge in startTri.GetEdges())
                {
                    // Initialize loop variables
                    bool speculating = false;

                    Triangle nextTri = startTri;
                    int nextIdx = nextTri.GetOppositeVertex(startEdge).Value;

                    List<int> indicies = new List<int> { startEdge.A, startEdge.B };
                    List<Triangle> possibleTris = remainingTris.ToList();

                    bool deadEdge = false;
                    do
                    {
                        Edge edge;
                        if (speculating)
                        {
                            // Switch active edge speculatively
                            edge = new Edge(indicies[indicies.Count - 2], indicies[indicies.Count - 1]);
                        }
                        else
                        {
                            // Active edge is between the previous and current vertices
                            edge = new Edge(indicies.Last(), nextIdx);

                            // Remove current triangle from selection pool
                            possibleTris.Remove(nextTri);

                            // Add current vertex to the strip
                            indicies.Add(nextIdx);
                        }

                        try
                        {
                            // Get the next triangle that shares the active edge with the current one
                            nextTri = possibleTris.First(tri => tri.GetEdges().Contains(edge));

                            // Next vertex in the strip lies opposite to the active edge
                            int? idx = nextTri.GetOppositeVertex(edge);
                            nextIdx = idx.Value;

                            // If we were speculatively adding a degenerate in an attempt
                            // to continue the strip, make sure to clear the flag
                            speculating = false;
                        }
                        catch (InvalidOperationException)
                        {
                            if (!speculating)
                            {
                                // If no "next" triangle could be found, speculatively 
                                // create a degenerate triangle in an attempt to make the strip longer
                                indicies.Insert(indicies.Count - 1, indicies[indicies.Count - 3]);
                                speculating = true;
                            }
                            else
                            {
                                // Otherwise, if attempted speculation fails, 
                                // remove the degenerate triangle and end the strip
                                indicies.RemoveAt(indicies.Count - 2);
                                speculating = false;
                                deadEdge = true;
                            }
                        }
                    } while (!deadEdge);

                    // Add candidate to the list
                    candidates.Add(new StripCandidate(indicies.ToArray(), possibleTris));
                }

                // Get the longest strip "candidate"
                var best = candidates.OrderBy(c => c.Indicies.Length).Last();

                // Create new triangle strip mesh
                var strip = new ConcreteTriangleStrip(Material, best.Indicies, vertices);
                strips.Add(strip);

                // Reduce the triangle pool according to 
                // what remained after creating the "best" strip
                remainingTris = best.Triangles;
            }

            return strips.ToArray();
        }

        public virtual async Task WriteToOBJAsync(StreamWriter writer, Matrix4x4? transform = null)
        {
            await writer.WriteLineAsync($"usemtl material_{Material.Id}");

            var verticies = await GetVerticesAsync();
            var tris = await GetTrianglesAsync();

            foreach (var v in verticies)
            {
                var pos = Vector3.Transform(v.Pos, transform ?? Matrix4x4.Identity);
                await writer.WriteLineAsync($"v {pos.X} {pos.Y} {pos.Z} {v.Color}");
            }

            foreach (var v in verticies)
            {
                await writer.WriteLineAsync($"vt {v.Tex.X} {v.Tex.Y}");
            }

            foreach (var tri in tris)
            {
                int a = tri.A - verticies.Length;
                int b = tri.B - verticies.Length;
                int c = tri.C - verticies.Length;

                await writer.WriteLineAsync($"f {a}/{a} {b}/{b} {c}/{c}");
            }
        }
    }
}
