﻿using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace WinUi.UiElements;
internal class ClickableAppBarButton : AppBarButton
{
    protected override void OnApplyTemplate()
    {
        this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        base.OnApplyTemplate();
    }
}
