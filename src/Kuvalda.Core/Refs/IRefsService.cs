namespace Kuvalda.Core
{
    public interface IRefsService
    {
        bool Exists(string name);
        Reference Get(string name);
        void Store(string name, Reference reference);
        Reference GetHead();
        string GetHeadCommit();
        void SetHead(Reference value);
    }
}