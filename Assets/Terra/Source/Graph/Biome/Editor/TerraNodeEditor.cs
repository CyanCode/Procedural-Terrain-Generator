using System;
using Terra.Graph;
using UnityEngine;
using XNodeEditor;

public class TerraNodeEditor: NodeEditor {
    public override GUIStyle GetBodyStyle() {
        GUIStyle style = base.GetBodyStyle();
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.active.textColor = Color.white;

        return style;
    }
}
