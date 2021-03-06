﻿using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class ObjectUtils
{
	public static T GetComponent<T>(this UnityEngine.GameObject owner, ref T field) 
		where T : Component
	{
		if (field != null)
			return field;
		field = owner.GetComponent<T> ();
		return field;
	}

	/// <summary>
	/// Get the current time since application startup.
	/// Valid also in Editor.
	/// </summary>
	public static float GetGlobalTime() {
		if (Application.isPlaying)
			return Time.realtimeSinceStartup;
		return (float)EditorApplication.timeSinceStartup;
	}
	
	public const HideFlags DynamicObjectHideFlags = HideFlags.DontSave | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

	public static bool IsDynamicObject (this UnityEngine.Object instance)
	{
		return (instance.hideFlags & DynamicObjectHideFlags) == DynamicObjectHideFlags;
	}

	public static void SetObjectAsDynamic (UnityEngine.Object instance)
	{
		if (instance != null)
			instance.hideFlags |= DynamicObjectHideFlags;
	}

	public static bool SafeDestroy (this UnityEngine.Object instance)
	{
		if (instance == null)
			return false;

		if (Application.isPlaying) {
			UnityEngine.Object.Destroy (instance);
		} else {
			UnityEngine.Object.DestroyImmediate (instance);
		}
		return true; 
	}

	private static void GetAllDynamicChildren(GameObject gameObject, ref List<GameObject> objects) {
		foreach (var item in gameObject.transform) {
			var transform = item as Transform;
			if (transform != null) {
				var go = transform.gameObject;
				if (go != null) {
					GetAllDynamicChildren (go, ref objects);

					if (go.IsDynamicObject ()) {
						if (objects == null)
							objects = new List<GameObject> ();
						objects.Add (go);
					}
				}
			}
		}

	}

	public static void DestroyDynamicChildrenRecursive (GameObject gameObject)
	{
		List<GameObject> objects = null;
		GetAllDynamicChildren (gameObject, ref objects);
		if (objects != null)
			foreach (var child in objects)
				SafeDestroy(child);
	}
}


