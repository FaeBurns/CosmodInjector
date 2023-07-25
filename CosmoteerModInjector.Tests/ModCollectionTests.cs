using CosmoteerModLib;
using Newtonsoft.Json;

namespace CosmoteerModInjector.Tests;

// ReSharper disable StringLiteralTypo
[TestFixture]
public class ModCollectionTests
{
    private static List<ModInfo> GetTestModInfo(string file)
    {
        return JsonConvert.DeserializeObject<List<ModInfo>>(File.ReadAllText($"Resources\\{file}.json"))!;
    }

    [Test]
    public void ValidSortTest()
    {
        ModCollection collection = new ModCollection();
        collection.AddRange(GetTestModInfo("testmodinfos_valid"));

        bool result = collection.TrySortInPlace(out List<ModDependencyError> errors);
        Assert.IsTrue(result);
        Assert.IsEmpty(errors);

        Assert.That(collection.Order[0].ModName, Is.EqualTo("test.mod.1"));
        Assert.That(collection.Order[1].ModName, Is.EqualTo("test.mod.3"));
        Assert.That(collection.Order[2].ModName, Is.EqualTo("test.mod.2"));
    }

    [Test]
    public void MissingSortTest()
    {
        ModCollection collection = new ModCollection();
        collection.AddRange(GetTestModInfo("testmodinfos_missing"));

        bool result = collection.TrySortInPlace(out List<ModDependencyError> errors);
        Assert.IsFalse(result);

        Assert.That(errors.Count, Is.EqualTo(1));
        Assert.That(errors[0].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.MISSING));
    }

    [Test]
    public void CyclicSortTest()
    {
        ModCollection collection = new ModCollection();
        collection.AddRange(GetTestModInfo("testmodinfos_cycle"));

        bool result = collection.TrySortInPlace(out List<ModDependencyError> errors);
        Assert.IsFalse(result);

        Assert.That(errors.Count, Is.EqualTo(2));
        Assert.That(errors[0].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
        Assert.That(errors[1].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
    }

    [Test]
    public void DoubleCyclicSortTest()
    {
        ModCollection collection = new ModCollection();
        collection.AddRange(GetTestModInfo("testmodinfos_twocycles"));

        bool result = collection.TrySortInPlace(out List<ModDependencyError> errors);
        Assert.IsFalse(result);

        Assert.That(errors.Count, Is.EqualTo(4));
        Assert.That(errors[0].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
        Assert.That(errors[1].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
        Assert.That(errors[2].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
        Assert.That(errors[3].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
    }

    [Test]
    public void GappedCyclicSortTest()
    {
        ModCollection collection = new ModCollection();
        collection.AddRange(GetTestModInfo("testmodinfos_cycle_gapped"));

        bool result = collection.TrySortInPlace(out List<ModDependencyError> errors);
        Assert.IsFalse(result);

        Assert.That(errors.Count, Is.EqualTo(3));
        Assert.That(errors[0].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
        Assert.That(errors[1].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
        Assert.That(errors[2].Type, Is.EqualTo(ModDependencyError.ModDependencyErrorReason.CYCLICAL_DEPENDENCY));
    }
}