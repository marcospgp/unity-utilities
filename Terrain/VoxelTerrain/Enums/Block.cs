namespace UnityUtilities.Terrain
{
    /// <summary>
    /// Using byte to save space as we don't expect > 256 blocks for now.
    /// </summary>
    public enum Block : byte
    {
        Air,
        Dirt,
        Grass,
        Sand,
        Stone,
    }
}
