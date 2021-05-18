namespace ArcticFox.Net.Util
{
    public class FixedSizeHeader : SizeBufferer
    {
        public FixedSizeHeader(int size)
        {
            m_resetSizeAfterUse = false; // size wont change
            SetSize(size);
        }
    }
}