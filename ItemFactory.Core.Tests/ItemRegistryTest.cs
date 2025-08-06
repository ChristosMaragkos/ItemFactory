using System.Collections.Generic;
using System.Linq;
using ItemFactory.Core.Interfaces;
using ItemFactory.Core.Loading;
using Xunit;

namespace ItemFactory.Core.Tests;

public class ItemRegistryTest
{

    private class TestItemSettings : AbstractItemSettings
    {
        private int _maxStackSize = 99;
        private bool _isFlammable;

        public int GetMaxStackSize() => _maxStackSize;
        public bool IsFlammable() => _isFlammable;
        
        public TestItemSettings MaxStackSize(int value)
        {
            _maxStackSize = value;
            return this;
        }
        
        public TestItemSettings Flammable()
        {
            _isFlammable = true;
            return this;
        }
    }

    private class TestItem : IBaseItem
    {
        public string Name { get; }
        public string Namespace { get; }
        public AbstractItemSettings Settings { private get; init; }
        
        public TestItem(string @namespace, string name, AbstractItemSettings settings)
        {
            Name = name;
            Namespace = @namespace;
            Settings = settings;
        }

        private TestItemSettings GetSettings()
        {
            return Settings as TestItemSettings;
        }
        
        public int GetMaxStackSize()
        {
            return GetSettings().GetMaxStackSize();
        }
        
        public bool IsFlammable()
        {
            return GetSettings().IsFlammable();
        }
    }

    private static class TestItems
    {
        private static readonly TestItemSettings DefaultSettings = new TestItemSettings()
            .MaxStackSize(64)
            .Flammable();
        
        private static readonly TestItemSettings DefaultSettingsNoFlammable = new TestItemSettings()
            .MaxStackSize(64);
        
        public static TestItem Apple { get; private set; }
        public static TestItem Pear { get; private set; }
        public static TestItem AnotherApple { get; private set; }

        public static void InitItems()
        {
            Apple = ItemRegistry.Register(new TestItem(
                "test",
                "apple",
                DefaultSettings));
            
            Pear = ItemRegistry.Register(new TestItem(
                "test",
                "pear",
                DefaultSettings));
            
            AnotherApple = ItemRegistry.Register(new TestItem(
                "test",
                "apple",
                DefaultSettingsNoFlammable));
        }
    }

    private static void Initialize(ConflictPolicy conflictPolicy = ConflictPolicy.KeepExisting)
    {
        ItemRegistry.Clear();
        ItemRegistry.Initialize(conflictPolicy);
        TestItems.InitItems();
    }

    [Fact]
    public void RegisterItem_Method_ShouldReturnValidReference()
    {
        Initialize();
        
        List<string> cachedIds = [];
        List<string> expectedIds =
        [
            "test:apple",
            "test:pear"
        ];

        cachedIds.AddRange(ItemRegistry.ToList().Select(item => item.GetId()));
        
        Assert.Equal(expectedIds, cachedIds);
    }

    [Fact]
    public void KeepExisting_ConflictPolicy_ShouldKeepFirstRegisteredItem()
    {
        Initialize();

        var item = ItemRegistry.ToList().First(i => i.GetId() == "test:apple") as TestItem;
        Assert.NotNull(item);
        Assert.True(item.IsFlammable(), "The kept item should be flammable " +
                                        "(the first registered Apple)");
        Assert.Equal(2, ItemRegistry.ToList().Count);
    }
    
    [Fact]
    public void OverwriteConflictPolicy_ShouldOverwriteExistingItem()
    {
        Initialize(ConflictPolicy.Overwrite);
        
        var item = ItemRegistry.ToList().First(i => i.GetId() == "test:apple") as TestItem;
        Assert.NotNull(item);
        Assert.False(item.IsFlammable(), "The overwritten item should not be flammable " +
                                        "(the second registered Apple)");
        Assert.Equal(2, ItemRegistry.ToList().Count);
    }

    [Fact]
    public void RemoveBothConflictPolicy_ShouldRemoveBothItems()
    {
        Initialize(ConflictPolicy.RemoveBoth);
        Assert.DoesNotContain(TestItems.Apple, ItemRegistry.ToList());
        Assert.DoesNotContain(TestItems.AnotherApple, ItemRegistry.ToList());
        Assert.Equal(1, ItemRegistry.ToList().Count); // Only Pear should remain
        Assert.Contains(TestItems.Pear, ItemRegistry.ToList());
    }
    
    [Fact]
    public void ItemSettings_ShouldBeAccessible()
    {
        Initialize();

        var apple = TestItems.Apple;
        Assert.NotNull(apple);
        Assert.Equal(64, apple.GetMaxStackSize());
        Assert.True(apple.IsFlammable());

        var pear = TestItems.Pear;
        Assert.NotNull(pear);
        Assert.Equal(64, pear.GetMaxStackSize());
        Assert.True(pear.IsFlammable());
    }
}