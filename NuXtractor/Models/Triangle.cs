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

using System;
using System.Collections.Generic;

namespace NuXtractor.Models
{
    public struct Edge : IEquatable<Edge>
    {
        public int A { get; }
        public int B { get; }

        public Edge(int a, int b)
        {
            A = a;
            B = b;
        }

        public bool Equals(Edge other)
        {
            return (A == other.A && B == other.B) || (A == other.B && B == other.A);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Edge) return Equals((Edge)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode();
        }

        public static bool operator ==(Edge a, Edge b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Edge a, Edge b)
        {
            return !a.Equals(b);
        }
    }

    public struct Triangle : IEquatable<Triangle>
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }

        public Edge E1 { get; }
        public Edge E2 { get; }
        public Edge E3 { get; }

        public Triangle(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;

            E1 = new Edge(a, b);
            E2 = new Edge(c, a);
            E3 = new Edge(b, c);
        }

        public Edge? GetOppositeEdge(int x)
        {
            if (A == x) return E3;
            else if (B == x) return E2;
            else if (C == x) return E1;
            else return null;
        }

        public int? GetOppositeVertex(Edge x)
        {
            if (E1 == x) return C;
            else if (E2 == x) return B;
            else if (E3 == x) return A;
            else return null;
        }

        public HashSet<Edge> GetEdges(int x)
        {
            if (A == x) return new HashSet<Edge> { E1, E2 };
            else if (B == x) return new HashSet<Edge> { E2, E3 };
            else if (C == x) return new HashSet<Edge> { E3, E1 };
            else return null;
        }

        public HashSet<Edge> GetEdges()
        {
            return new HashSet<Edge> { E1, E2, E3 };
        }

        public HashSet<int> GetVerticies(int x)
        {
            if (A == x) return new HashSet<int> { B, C };
            else if (B == x) return new HashSet<int> { C, A };
            else if (C == x) return new HashSet<int> { A, B };
            else return null;
        }

        public HashSet<int> GetVerticies()
        {
            return new HashSet<int> { A, B, C };
        }

        public bool Equals(Triangle other)
        {
            return GetEdges().SetEquals(other.GetEdges());
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Triangle) return Equals((Triangle)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() + B.GetHashCode() + C.GetHashCode();
        }

        public static bool operator ==(Triangle a, Triangle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Triangle a, Triangle b)
        {
            return !a.Equals(b);
        }
    }
}
