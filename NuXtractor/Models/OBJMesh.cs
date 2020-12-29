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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor.Models
{
    public class OBJMesh : Model
    {
        private StreamReader Reader { get; }

        private List<Triangle> Triangles { get; }
        private List<Vertex> Vertices { get; }

        private List<Vector3> Points { get; }
        private List<Vector2> Coords { get; }

        public OBJMesh(StreamReader reader) : base(new Material(0, new Color(1.0f, 1.0f, 1.0f, 1.0f), null))
        {
            Reader = reader;

            Coords = new List<Vector2>();
            Points = new List<Vector3>();

            Triangles = new List<Triangle>();
            Vertices = new List<Vertex>();
        }

        public async Task ParseAsync()
        {
            var pointTris = new List<Triangle>();
            var coordTris = new List<Triangle>();

            while (!Reader.EndOfStream)
            {
                var line = await Reader.ReadLineAsync();
                var tokens = line.Split(' ');

                switch (tokens[0])
                {
                    case "v":
                        float x = float.Parse(tokens[1]);
                        float y = float.Parse(tokens[2]);
                        float z = float.Parse(tokens[3]);
                        Points.Add(new Vector3(x, y, z));
                        break;
                    case "vt":
                        float u = float.Parse(tokens[1]);
                        float v = float.Parse(tokens[2]);
                        Coords.Add(new Vector2(u, v));
                        break;
                    case "f":
                        int[] pIdx = new int[3];
                        int[] cIdx = new int[3];
                        for (int i = 1; i < 4; i++)
                        {
                            var subTokens = tokens[i].Split('/');

                            pIdx[i - 1] = int.Parse(subTokens[0]) - 1;

                            if (subTokens.Length > 1)
                                cIdx[i - 1] = int.Parse(subTokens[1]) - 1;
                            else
                                cIdx[i - 1] = -1;

                        }

                        var pTri = new Triangle(pIdx[0], pIdx[1], pIdx[2]);
                        var cTri = new Triangle(cIdx[0], cIdx[1], cIdx[2]);

                        pointTris.Add(pTri);
                        coordTris.Add(cTri);
                        break;
                }
            }

            var pairs = pointTris.SelectMany(t => new int[] { t.A, t.B, t.C })
                .Zip(coordTris.SelectMany(t => new int[] { t.A, t.B, t.C }),
                    (p, c) => (p, c));

            var uniquePairs = pairs
                .GroupBy(x => x.p)
                .Select(g => g.First());

            Vertices.AddRange(
                uniquePairs.Select(
                    x => new Vertex(Points[x.p], x.c < 0 ? new Vector2() : Coords[x.c], new Color())
                    )
                );

            var uniqueIndicies = uniquePairs
                .Select(x => x.p)
                .ToList();

            Triangles.AddRange(pairs
                .Select(x => uniqueIndicies.IndexOf(x.p))
                .Select((n, i) => new { n, i })
                .GroupBy(x => x.i / 3, x => x.n)
                .Select(g => g.ToArray())
                .Select(g => new Triangle(g[0], g[1], g[2]))
                );
        }

        public override Task<Vertex[]> GetVerticesAsync()
        {
            return Task.FromResult(Vertices.ToArray());
        }

        public override Task<Triangle[]> GetTrianglesAsync()
        {
            return Task.FromResult(Triangles.ToArray());
        }
    }
}
