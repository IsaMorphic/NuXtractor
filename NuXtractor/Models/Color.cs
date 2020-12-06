namespace NuXtractor.Models
{
    public struct Color
    {
        public float Red { get; }
        public float Green { get; }
        public float Blue { get; }

        public float Alpha { get; }

        public Color(float red, float green, float blue, float alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;

            Alpha = alpha;
        }

        public override string ToString()
        {
            return $"{Red} {Green} {Blue}";
        }
    }
}
