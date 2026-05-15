namespace RjClicker.App.Infrastructure.PInvoke;

public static class NativeConstants
{
    public const uint InputMouse = 0;

    public const uint MouseEventLeftDown = 0x0002;
    public const uint MouseEventLeftUp = 0x0004;
    public const uint MouseEventRightDown = 0x0008;
    public const uint MouseEventRightUp = 0x0010;

    public const uint WmLButtonDown = 0x0201;
    public const uint WmLButtonUp = 0x0202;
    public const uint WmRButtonDown = 0x0204;
    public const uint WmRButtonUp = 0x0205;

    public const uint MkLButton = 0x0001;
    public const uint MkRButton = 0x0002;

    public const int WhMouseLl = 14;
}