
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

namespace UnityEditor.Experimental.TerrainAPI
{
    public delegate void ResetBrush();
	public interface IBrushUIGroup {
		/// <summary>
		/// The normalized size of the brush.
		/// </summary>
		float brushSize { get; }
		
		/// <summary>
		/// The rotation of the brush (in degrees).
		/// </summary>
		float brushRotation { get; }
		
		/// <summary>
		/// The normalized strength of the brush when applied.
		/// </summary>
		float brushStrength { get; }
		
		/// <summary>
		/// The spacing used when applying certain brushes.
		/// </summary>
		float brushSpacing { get;  }


        string validationMessage { get; set; }
        /// <summary>
        /// Are we allowed to paint with this brush?
        /// </summary>
        bool allowPaint { get; }
		
		bool InvertStrength { get; }
		bool isInUse { get; }

        FilterStackView brushMaskFilterStackView { get; }
        FilterStack brushMaskFilterStack { get; }


        Terrain terrainUnderCursor { get; }
		bool isRaycastHitUnderCursorValid { get; }
		RaycastHit raycastHitUnderCursor { get; }

		void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext);
		void OnEnterToolMode();
		void OnExitToolMode();
		void OnPaint(Terrain terrain, IOnPaint editContext);
		void OnSceneGUI2D(Terrain terrain, IOnSceneGUI editContext);
        void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext);
		void AppendBrushInfo(Terrain terrain, IOnSceneGUI editContext, StringBuilder builder);
        RenderTexture GetBrushMask(FilterContext fc, RenderTexture heightContext);

        /// <summary>
        /// Scatters the brush around the specified UV on the specified terrain. If the scattered UV leaves
        /// the current terrain then the terrain AND UV are modified for the terrain the UV is now over.
        /// </summary>
        /// <param name="terrain">The terrain the scattered UV co-ordinate is actually on.</param>
        /// <param name="uv">The UV co-ordinate passed in transformed into the UV co-ordinate relative to the scattered terrain.</param>
        /// <returns>"true" if we scattered to a terrain, "false" if we fell off ALL terrains.</returns>
        bool ScatterBrushStamp(ref Terrain terrain, ref Vector2 uv);
		
		bool ModifierActive(BrushModifierKey k);

	}
}
