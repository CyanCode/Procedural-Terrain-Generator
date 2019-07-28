using System;
using UnityEngine;
using System.Linq;

namespace Terra.Structures {
	[Serializable]
	public class LodData {
		/// <summary>
		/// Represents a level of detail
		/// </summary>
		[Serializable]
		public class Lod {
			public int StartRadius;
			public int Resolution;

			public Color PreviewColor {
				get {
					switch (StartRadius) {
						case 0:
							return Color.green;
						case 1:
							return Color.yellow;
						case 2:
							return Color.red;
					}

					if (_previewColor == default(Color)) {
						_previewColor = UnityEngine.Random.ColorHSV();
					}

					return _previewColor;
				}
			}

			/// <summary>
			/// Internal <see cref="PreviewColor"/>
			/// </summary>
			[SerializeField]
			private Color _previewColor;

			public Lod(int startRadius, int resolution) {
				StartRadius = startRadius;
				Resolution = resolution;
			}
		}

		public LodData() {
			if (LevelsOfDetail == null) {
				LevelsOfDetail = new Lod[1];
				LevelsOfDetail[0] = new Lod(0, 129);
			}
		}

		/// <summary>
		/// Represents the various levels of detail assigned to this LodData instance.
		/// </summary>
		public Lod[] LevelsOfDetail;

		/// <summary>
		/// Get the Lod associated with the passed radius. If 
		/// no level matches, a LOD with a resolution 128 is returned.
		/// </summary>
		/// <param name="radius">Radius to look for</param>
		public Lod GetLevelForRadius(int radius) {
			foreach (var lvl in LevelsOfDetail.Reverse()) {
				if (lvl.StartRadius < radius) {
					return lvl;
				}
			}

			return new Lod(0, 129);
		}

		/// <summary>
		/// Get the LOD associated with the passed position in world space. This 
		/// checks for grid intersections with <see cref="GenerationData.LodChangeRadius"/>.
		/// </summary>
		/// <param name="gridPosition">GridPosition to get a LOD for</param>
		/// <param name="position">Position of whatever object is being tracked</param>
		/// <returns></returns>
		public Lod GetLevelForPosition(GridPosition gridPosition, Vector3 position) {
			Vector2 circCenter = new Vector2(position.x, position.z);
			float radius = TerraConfig.Instance.Generator.LodChangeRadius;

			if (SquareIntersectsCircle(circCenter, radius, gridPosition)) {
				return LevelsOfDetail[0];
			}

			//Default to getting level from radius if no intersection
			int length = TerraConfig.Instance.Generator.Length;
			Vector2 worldXz = new Vector2(position.x, position.z);
			return GetLevelForRadius((int)gridPosition.Distance(new GridPosition(worldXz, length)));
		}

		/// <summary>
		/// Adjusts the internal <see cref="LevelsOfDetail"/> array to match 
		/// the passed count. If the passed count is greater than <see cref="LevelsOfDetail"/>'s 
		/// count, <see cref="LevelsOfDetail"/> is expanded. If it is 
		/// less than <see cref="LevelsOfDetail"/>'s count, <see cref="LevelsOfDetail"/> is 
		/// truncated to match the passed count.
		/// </summary>
		/// <param name="count">Count to match <see cref="LevelsOfDetail"/> with.</param>
		public void AdjustLevelsToCount(int count) {
			if (LevelsOfDetail == null) {
				LevelsOfDetail = new Lod[count];
			}

			if (count > LevelsOfDetail.Length) {
				Lod[] tmp = new Lod[count];
				LevelsOfDetail.CopyTo(tmp, 0);

				//New indices should have an increasing StartRadius
				int lastStartRadius = LevelsOfDetail[count - (count - LevelsOfDetail.Length) - 1].StartRadius;
				for (int i = LevelsOfDetail.Length; i < count; i++) {
					lastStartRadius++;
					tmp[i] = new Lod(lastStartRadius, 129);
				}

				LevelsOfDetail = tmp;
			}
			if (count < LevelsOfDetail.Length) {
				Lod[] tmp = new Lod[count];
				for (int i = 0; i < count; i++) {
					tmp[i] = LevelsOfDetail[i];
				}

				LevelsOfDetail = tmp;
			}
		}

		/// <summary>
		/// Sorts <see cref="LevelsOfDetail"/> by <see cref="Lod.StartRadius"/>.
		/// </summary>
		public void SortByStartRadius() {
			LevelsOfDetail = LevelsOfDetail.OrderBy(lod => lod.StartRadius).ToArray();
		}

		private bool SquareIntersectsCircle(Vector2 circCenter, float cirRadius, GridPosition square) {
			int length = TerraConfig.Instance.Generator.Length;
			Rect rect = new Rect {
				x = length * square.X,
				y = length * square.Z ,
				height = length,
				width = length
			};

			Vector2 circleDistance = Vector2.zero;

			circleDistance.x = Mathf.Abs(circCenter.x - rect.x);
			circleDistance.y = Mathf.Abs(circCenter.y - rect.y);

			if (circleDistance.x > (rect.width / 2 + cirRadius)) { return false; }
			if (circleDistance.y > (rect.height / 2 + cirRadius)) { return false; }

			if (circleDistance.x <= (rect.width / 2)) { return true; }
			if (circleDistance.y <= (rect.height / 2)) { return true; }

			float cornerDistanceSq = Mathf.Pow(circleDistance.x - rect.width / 2, 2) +
			                    Mathf.Pow(circleDistance.y - rect.height / 2, 2);

			return (cornerDistanceSq <= Mathf.Pow(cirRadius, 2));
		} 
	}
}
