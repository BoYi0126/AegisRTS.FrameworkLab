namespace AegisRTS.Gameplay.Content
{
    /// <summary>Checks whether an authored prefab ID resolves in the host project.</summary>
    public interface IContentAssetCatalog
    {
        bool ContainsPrefab(string prefabId);
    }
}
