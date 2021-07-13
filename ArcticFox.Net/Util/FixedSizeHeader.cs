namespace ArcticFox.Net.Util
{
    public class FixedSizeHeader<T> : SizeBufferer<T>
    {
        public FixedSizeHeader(int size)
        {
            m_resetSizeAfterUse = false; // size wont change
            SetSize(size);
        }
    }
}