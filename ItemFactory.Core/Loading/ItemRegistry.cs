using ItemFactory.Core.Interfaces;

namespace ItemFactory.Core.Loading;

public static class ItemRegistry
{
    private static readonly Dictionary<string, IBaseItem> Items = new();
    
    private static ConflictPolicy _conflictPolicy = ConflictPolicy.KeepExisting;

    public static TItem? Register<TItem>(TItem item)
    where TItem : class, IBaseItem
    {
        var id = item.GetId();
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Item ID cannot be null or empty.", item.Name);
        }
        
        if (Items.TryAdd(id, item)) return item;
        switch (_conflictPolicy)
        {
            case ConflictPolicy.KeepExisting:
                return Items[id] as TItem;
            case ConflictPolicy.Overwrite:
                Items[id] = item;
                return item;
            case ConflictPolicy.RemoveBoth:
                Items.Remove(id);
                return null;
            default:
                throw new ArgumentOutOfRangeException(_conflictPolicy.ToString());
        }
    }
    
    /// <summary>
    /// Static method to initialize the item registry.
    /// You can optionally specify a conflict policy for handling item ID conflicts for
    /// purposes such as overwriting existing items or removing both conflicting items via mods.
    /// If no policy is specified, it defaults to keeping the first item registered with that ID.
    ///
    /// Important: Ensure that you call this method within a game initializer before any items are registered.
    /// </summary>
    /// <param name="conflictPolicy">The item ID conflict handling policy.
    /// Keeps the first item by default</param>
    public static void Initialize(ConflictPolicy conflictPolicy = ConflictPolicy.KeepExisting)
    {
        _conflictPolicy = conflictPolicy;
    }
    
    #if DEBUG
    public static void Clear()
    {
        Items.Clear();
    }

    public static List<IBaseItem> ToList()
    {
        return Items.Values.ToList();
    }
    #endif
}