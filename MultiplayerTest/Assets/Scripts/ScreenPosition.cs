using UnityEngine;

public static class ScreenPosition
{
    public static Vector3 ZeroFromBottomLeftToCenter(Vector3 position)
    {
        position.x -= Screen.width / 2;
        position.y -= Screen.height / 2;
        position.z = 0;
        return position;
    }
    public static Vector3 ZeroFromCenterToBottomLeft(Vector3 position)
    {
        position.x += Screen.width / 2;
        position.y += Screen.height / 2;
        position.z = 0;
        return position;
    }
}
