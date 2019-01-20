namespace Kuvalda.Core
{
    public interface IRefsService
    {
        bool Exists(string name);
        string Get(string name);
        string Store(string name, string chash);
        string GetHeadCommit();
        string GetGetHeadRefName();
        string SetHeadCommit(string chash);
        string SetHeadRef(string refName);
    }
}