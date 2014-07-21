namespace Conejo
{
    public interface ISerializer
    {
        string Serialize(object @object);
        T Deserialize<T>(string source);
    }
}
