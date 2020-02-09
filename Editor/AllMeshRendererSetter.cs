/*
## License
Copyright (c) 2020 YukiYukiVirtual
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AllMeshRendererSetter : EditorWindow
{
	// 設定可能な項目
	private static GameObject _parentObject;
	private static GameObject _rootBone; 
	private static GameObject _anchor;
	private static Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 2);
	
	// レンダラーがついているオブジェクトリスト
	private static List<GameObject> _skinnedMeshObjects;
	private static List<GameObject> _meshObjects;
	
	// ヒエラルキーのオブジェクト右クリックで使えるやつ
	[MenuItem ("GameObject/AllMeshRendererSetter", false, 20)]
	public static void ShowWindow () {
		// 右クリックした要素を設定
		_parentObject = Selection.activeGameObject;
		
		EditorWindow.GetWindow (typeof (AllMeshRendererSetter));
	}
	
	// 出てくるウィンドウ
	private void OnGUI()
	{
		EditorGUILayout.BeginVertical(GUI.skin.box);
		
		EditorGUILayout.BeginHorizontal();	// アバターのPrefab
		_parentObject = (GameObject)EditorGUILayout.ObjectField("Prefab", _parentObject, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();	// RootBone
		_rootBone = (GameObject)EditorGUILayout.ObjectField("Root Bone(Hip)", _rootBone, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();	// Prove Anchor
		_anchor = (GameObject)EditorGUILayout.ObjectField("Prove Anchor(Chest)", _anchor, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();	// Bounds
		_bounds = EditorGUILayout.BoundsField("Bounds", _bounds);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		if (GUILayout.Button("設定する"))
		{
			exec();
		}
	}

	// 削除処理
	private void exec()
	{
		// パラメータチェック
		if(_parentObject == null || _rootBone == null || _anchor == null)
		{
			EditorUtility.DisplayDialog("エラー", "パラメータがnullです", "OK");
			return;
		}

		// リスト初期化
		_skinnedMeshObjects = new List<GameObject>();
		_meshObjects = new List<GameObject>();
		
		// メッシュを取得
		getAllMeshes();
		
		// 設定する
		foreach(GameObject obj in _skinnedMeshObjects)
		{
			var skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();
			var transform = obj.transform;
			
			// undoできるようにするやつ
			Undo.RecordObject(skinnedMesh, "SETTING " + skinnedMesh.name);
			Undo.RecordObject(transform, "SETTING " + transform.name);
			
			// Transformリセット
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale    = Vector3.one;
			
			skinnedMesh.rootBone = _rootBone.transform;
			skinnedMesh.localBounds = _bounds;
			skinnedMesh.probeAnchor = _anchor.transform;

		}
		foreach(GameObject obj in _meshObjects)
		{
			var mesh = obj.GetComponent<MeshRenderer>();
			
			Undo.RecordObject(mesh, "SETTING " + mesh.name);
			
			mesh.probeAnchor = _anchor.transform;
		}
		
		EditorUtility.DisplayDialog("完了", "設定が完了しました", "OK");
		return;
	}

	
	// メッシュを取得
	private static void getAllMeshes()
	{
		getChildren(_parentObject.transform);
	}
	
	private static void getChildren(Transform t)
	{
		Transform children = t.GetComponentInChildren<Transform>();
		foreach (Transform child in children)
		{
			// SkinnedMeshRendererを追加する
			if(child.GetComponent<SkinnedMeshRenderer>())
			{
				_skinnedMeshObjects.Add(child.gameObject);
			}
			// MeshRendererを追加する
			if(child.GetComponent<MeshRenderer>())
			{
				_meshObjects.Add(child.gameObject);
			}
			getChildren(child);
		}
	}

}
