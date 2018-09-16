using System.Collections.Generic;
using Terra.Data;
using Terra.ReorderableList;
using Terra.Terrain;
using UnityEngine;

namespace UnityEditor.Terra {
	public class ReorderableMaterialList: GenericListAdaptor<SplatData> {
		private DetailData _detail;

		const float MAX_HEIGHT = 200f;
		const int PADDING = 8;
		const int PADDING_SM = 4;
		const int CTRL_HEIGHT = 16; //Default height for controls
		const int TEX_HEIGHT = 60;
		const int TEX_CAPTION_HEIGHT = 16;

		public ReorderableMaterialList(TerraSettings settings, DetailData detail) : base(detail.SplatsData, null, MAX_HEIGHT) {
			_detail = detail;
		}

		public override void DrawItem(Rect position, int index) {
			//To-be modified original-- caching original position
			Rect pos = position;
			var splat = this[index];

			//Insert top padding & set height
			pos.y += 8f;
			pos.height = CTRL_HEIGHT * 2.5f;

			if (splat.Diffuse == null) {
				EditorGUI.HelpBox(pos, "This splat material does not have a selected diffuse texture.", MessageType.Warning);
				pos.y += CTRL_HEIGHT * 2.5f + PADDING_SM;
			} else {
				float texWidth = (pos.width / 2) - PADDING;

				Rect texPos = new Rect(pos.x, pos.y, texWidth, TEX_HEIGHT);
				Rect labelPos = new Rect(texPos.x, texPos.y + TEX_HEIGHT + PADDING_SM, texWidth, TEX_CAPTION_HEIGHT);

				GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
				centeredStyle.alignment = TextAnchor.UpperCenter;

				EditorGUI.ObjectField(texPos, splat.Diffuse, typeof(Texture2D), false);
				EditorGUI.LabelField(labelPos, "Diffuse", centeredStyle);

				if (splat.Normal != null) {
					texPos.x += texPos.width + 8;
					labelPos.x += texPos.width + PADDING;

					splat.Normal = (Texture2D)EditorGUI.ObjectField(texPos, splat.Normal, typeof(Texture2D), false);
					EditorGUI.LabelField(labelPos, "Normal", centeredStyle);
				}

				//Update position
				pos.y += TEX_HEIGHT + labelPos.height + PADDING;
			}

			//Reset referenced height
			pos.height = CTRL_HEIGHT;

			if (GUI.Button(new Rect(pos.x, pos.y, pos.width, pos.height + 2), "Edit Material")) {
				AddTextureWindow.Init(ref splat);
			}
			pos.y += CTRL_HEIGHT + PADDING;
			
			//Blend factor
			splat.Blend = EditorGUI.FloatField(pos, "Blend Amount", splat.Blend);
			pos.y += CTRL_HEIGHT + PADDING_SM;

			//Constrain Height
			splat.ConstrainHeight = EditorGUI.Toggle(pos, "Constrain Height", splat.ConstrainHeight);
			pos.y += CTRL_HEIGHT + PADDING_SM;
			if (splat.ConstrainHeight) {
				EditorGUI.indentLevel++;
				splat.HeightConstraint = EditorGUIExtension.DrawConstraintRange(pos, "Height", splat.HeightConstraint, 0, 1);

				pos.y += (CTRL_HEIGHT + PADDING_SM) * 2;

				float startX = pos.x;
				pos.x = EditorGUI.IndentedRect(pos).x;
				EditorGUI.indentLevel--;

				//Checkboxes for infinity & -infinity heights
				EditorGUI.BeginChangeCheck();
				if (splat.IsMaxHeight || !_detail.IsMaxHeightSelected) {
					splat.IsMaxHeight = EditorGUI.Toggle(pos, "Is Highest Material", splat.IsMaxHeight);
					pos.y += CTRL_HEIGHT + PADDING_SM;
				}
				if (EditorGUI.EndChangeCheck()) {
					_detail.IsMaxHeightSelected = splat.IsMaxHeight;
				}

				EditorGUI.BeginChangeCheck();
				if (splat.IsMinHeight || !_detail.IsMinHeightSelected) {
					splat.IsMinHeight = EditorGUI.Toggle(pos, "Is Lowest Material", splat.IsMinHeight);
					pos.y += CTRL_HEIGHT + PADDING_SM;
				}
				if (EditorGUI.EndChangeCheck()) {
					_detail.IsMinHeightSelected = splat.IsMinHeight;
				}

				pos.x = startX;
			}

			//Constrain Angle
			splat.ConstrainAngle = EditorGUI.Toggle(pos, "Constrain Angle", splat.ConstrainAngle);
			pos.y += CTRL_HEIGHT + PADDING_SM;
			if (splat.ConstrainAngle) {
				EditorGUI.indentLevel++;
				splat.AngleConstraint = EditorGUIExtension.DrawConstraintRange(pos, "Angle", splat.AngleConstraint, 0, 90);
				EditorGUI.indentLevel--;
			}
		}

		public override void Remove(int index) {
			//Remove max / min height bools if necessary
			var ss = this[index];

			if (ss.IsMaxHeight) {
				_detail.IsMaxHeightSelected = false;
			} 
			if (ss.IsMinHeight) {
				_detail.IsMinHeightSelected = false;
			}

			base.Remove(index);
		}

		public override float GetItemHeight(int index) {
			const int minHeight = (CTRL_HEIGHT * 5) + (PADDING_SM * 4);
			var splat = this[index];

			float height = minHeight;

			//Texture area height
			if (splat.Diffuse != null) {
				height += TEX_HEIGHT + PADDING_SM + TEX_CAPTION_HEIGHT;
			} else {
				height += CTRL_HEIGHT * 2.5f + PADDING_SM;
			}

			if (splat.ConstrainHeight) {
				height += (CTRL_HEIGHT * 4) + (PADDING_SM * 3);
			}
			if (splat.ConstrainAngle) {
				height += (CTRL_HEIGHT * 2) + (PADDING_SM * 2);
			}
			
			return height;
		}

		/// <summary>
		/// Cached texture used by <code>GetWhiteTexture</code> method
		/// </summary>
		protected static Texture2D WhiteTex;

		/// <summary>
		/// Gets a cached white texture that can be used for GUI
		/// </summary>
		/// <returns>All white Texture instance</returns>
		private static Texture2D GetWhiteTexture() {
			if (WhiteTex == null) {
				WhiteTex = new Texture2D(1, 1);
				WhiteTex.SetPixel(0, 0, new Color(230f / 255f, 230f / 255f, 230f / 255f));
				WhiteTex.Apply();
			}

			return WhiteTex;
		}
	}
}
