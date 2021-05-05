using UnityEditor;
using UnityEngine;

public static class EditorUtilities {
	
	/// Copy sprite texture within sprite rect
	public static Texture2D CopyTexture(Sprite sprite) {
		var newTexture = new Texture2D((int)sprite.textureRect.size.x, (int)sprite.textureRect.size.y, TextureFormat.RGBA32, false) 
			{filterMode = FilterMode.Point, alphaIsTransparency = true};
		Color[] pixels = sprite.texture.GetPixels(
			(int)sprite.textureRect.x,
			(int)sprite.textureRect.y,
			(int)sprite.textureRect.width,
			(int)sprite.textureRect.height);

		newTexture.SetPixels(pixels);
		newTexture.Apply();

		return newTexture;
	}

	/// Copy part of texture
	public static Texture2D CopyTexture(Texture2D texture, Rect rect) {
		var newTexture = new Texture2D((int)rect.size.x, (int)rect.size.y, TextureFormat.RGBA32, false) {filterMode = FilterMode.Point, alphaIsTransparency = true};
		Color[] pixels = texture.GetPixels(
			(int)rect.x,
			(int)rect.y,
			(int)rect.width,
			(int)rect.height);

		newTexture.SetPixels(pixels);
		newTexture.Apply();

		return newTexture;
	}
	
	/// Copy whole texture
	public static Texture2D CopyTexture(Texture2D texture) {
		var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false) {filterMode = FilterMode.Point, alphaIsTransparency = true};
		Color[] pixels = texture.GetPixels(
			0,
			0,
			texture.width,
			texture.height);

		newTexture.SetPixels(pixels);
		newTexture.Apply();

		return newTexture;
	}

	public static void DrawVerticalLine(float x, float startY, float endY, int lineWidth, Color color) {
		var lineRect = new Rect(x, startY,lineWidth, endY-startY);
		EditorGUI.DrawRect(lineRect, color);
	}
	
	public static void DrawHorizontalLine(float y, float startX, float endX, int lineHeight, Color color) {
		var lineRect = new Rect(startX, y, endX-startX, lineHeight);
		EditorGUI.DrawRect(lineRect, color);
	}
	
	public static Rect DrawOuterBorders(Rect rect, int borderWidth, Color color) {		
		var newRect = new Rect(rect.x - borderWidth, rect.y - borderWidth , rect.width + borderWidth*2, rect.height + borderWidth*2);
		
		if (Event.current?.type != EventType.Repaint)
			return newRect;

		if (borderWidth > 0) {
			// Left
			Rect rect1 = newRect;
			rect1.width = borderWidth;
			EditorGUI.DrawRect(rect1, color);
			
			// Top
			rect1 = newRect;
			rect1.height = borderWidth;
			EditorGUI.DrawRect(rect1, color);

			// Right
			rect1 = newRect;
			rect1.x += newRect.width - borderWidth;
			rect1.width = borderWidth;
			EditorGUI.DrawRect(rect1, color);
			
			// Bottom
			rect1 = newRect;
			rect1.y += newRect.height - borderWidth;
			rect1.height = borderWidth;
			EditorGUI.DrawRect(rect1, color);
		}
		return newRect;
	}
	
	public static Rect DrawInnerBorders(Rect rect, int borderWidth, Color color) {		
		var newRect = new Rect(rect.x + borderWidth, rect.y + borderWidth , rect.width - borderWidth*2, rect.height - borderWidth*2);
		if (Event.current?.type != EventType.Repaint)
			return newRect;

		if (borderWidth > 0)
		{
			Rect rect1 = rect;
			rect1.width = borderWidth;
			EditorGUI.DrawRect(rect1, color);
			
			rect1 = rect;
			rect1.height = borderWidth;
			EditorGUI.DrawRect(rect1, color);
			
			rect1 = rect;
			rect1.x += rect.width - borderWidth;
			rect1.width = borderWidth;
			EditorGUI.DrawRect(rect1, color);
			
			rect1 = rect;
			rect1.y += rect.height - borderWidth;
			rect1.height = borderWidth;
			EditorGUI.DrawRect(rect1, color);
		}
		return newRect;
	}
	
	public static void DrawTitle(string label, int size, int space, bool drawLine = true) {
		GUILayout.Space(space);
		var style = new GUIStyle {fontSize = size, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter};
		Rect r = EditorGUILayout.GetControlRect(false, size + space * 2);
		var labelRect = new Rect(r.x, r.y + space, r.width, size);
		EditorGUI.LabelField(labelRect, label, style);
		if (drawLine) {
			float width = r.width;
			var lineRect = new Rect(r.center.x - width/2, labelRect.y+size+(space/2), width, 0);
			DrawOuterBorders(lineRect, 1, Color.black);
		}
		GUILayout.Space(space);
	}

	public static void DrawBox(Vector2 size, Color color, float space) {
		Rect r = EditorGUILayout.GetControlRect(false, size.y);
		var rect = new Rect(r.x + space, r.y+1, size.x, size.y);
		EditorGUI.DrawRect(rect, color);
	}
}