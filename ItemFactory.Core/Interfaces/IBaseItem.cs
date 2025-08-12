namespace ItemFactory.Core.Interfaces;

public interface IBaseItem
{
    string Name { get; }
    string Namespace { get; }

    string GetId() => $"{Namespace}:{Name}";
}