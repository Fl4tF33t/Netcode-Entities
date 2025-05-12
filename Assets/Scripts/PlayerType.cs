using Unity.Mathematics;
using UnityEngine;

public enum PlayerType {
    None,
    Cross,
    Circle,
}

public struct Line {
    public int2 a;
    public int2 b;
    public int2 c;
    public Orientation orientation;
    public enum Orientation {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }
}

