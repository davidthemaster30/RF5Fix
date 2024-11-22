using UnityEngine;

namespace RF5Fix;

internal static class AspectRatio
{
    internal const float DefaultAspectRatio = (float)16 / 9;
    internal static readonly float ScreenAspectRatio = (float)Screen.width / Screen.height;
    internal static readonly float AspectMultiplier = ScreenAspectRatio / DefaultAspectRatio;
    internal static readonly float AspectDivider = DefaultAspectRatio / ScreenAspectRatio;

    internal static readonly bool ScreenRatioIsSmallerThenDefault = ScreenAspectRatio < DefaultAspectRatio;
    internal static readonly bool ScreenRatioIsBiggerThenDefault = ScreenAspectRatio > DefaultAspectRatio;
    internal static readonly bool ScreenRatioEqualToDefault = !ScreenRatioIsSmallerThenDefault && !ScreenRatioIsBiggerThenDefault;
}
