namespace Kuvalda.Core.Merge
{
    public class MergeConflict
    {
        public string Path { get; set; }
        public MergeConflictReason LeftReason { get; set; }
        public MergeConflictReason RightReason { get; set; }

        public MergeConflict(string path, MergeConflictReason leftReason, MergeConflictReason rightReason)
        {
            Path = path;
            LeftReason = leftReason;
            RightReason = rightReason;
        }

        protected bool Equals(MergeConflict other)
        {
            return string.Equals(Path, other.Path) && LeftReason == other.LeftReason && RightReason == other.RightReason;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MergeConflict) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) LeftReason;
                hashCode = (hashCode * 397) ^ (int) RightReason;
                return hashCode;
            }
        }
    }
}