namespace TobyCopy
{
    public class ResourceRange
    {
        public long start;
        public long length;

        public ResourceRange(long start, long length)
        {
            this.start = start;
            this.length = length;
        }

        public override bool Equals(object? obj)
        {
            return obj is ResourceRange range &&
                start == range.start &&
                length == range.length;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(start, length);
        }
    }
}
