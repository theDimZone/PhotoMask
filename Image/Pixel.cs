namespace photomask.Image
{
    public struct Pixel
    {
        public byte A { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public Pixel(byte a, byte r, byte g, byte b) => (A, R, G, B) = (a, r, g, b);
        public Pixel(int a, int r, int g, int b) => (A, R, G, B) = ((byte)a, (byte)r, (byte)g, (byte)b);
    }
}
