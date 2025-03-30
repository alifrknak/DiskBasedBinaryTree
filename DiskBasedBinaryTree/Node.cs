public struct Node
{
    public int Value { get; set; }

    /// <summary>
    /// point to node start position in the file
    /// </summary>
    public long Offset { get; set; }
    public long LeftOffset { get; set; }
    public long RightOffset { get; set; }

    public Node(int value, long leftOffset, long rightOffset)
    {
        Value = value;
        LeftOffset = leftOffset;
        RightOffset = rightOffset;
    }

    public Node(int value, long offset, long leftOffset, long rightOffset)
    {
        Value = value;
        Offset = offset;
        LeftOffset = leftOffset;
        RightOffset = rightOffset;
    }

    public static Node Create(int value)
    {
        return new Node(value, 0, 0);
    }
}
