using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
	public class PositionSaver : MonoBehaviour
	{
		[System.Serializable]
		public struct Data
		{
			public Vector3 Position;
			public float Time;
		}

		[SerializeField]
		[ReadOnly]
		[Tooltip("Для заполнения этого поля воспользуйтесь контекстным меню в инспекторе и командой \"Create File\"")]
		private TextAsset _json;

		[field: SerializeField, HideInInspector]
		public List<Data> Records { get; private set; }

		private void Awake()
		{
			//todo comment: Что будет, если в теле этого условия не сделать выход из метода?
			// ответ: дальше попробует прочитать _json.text у null и упадёт с ошибкой, поэтому return обязателен
			if (_json == null)
			{
				gameObject.SetActive(false);
				Debug.LogError("Please, create TextAsset and add in field _json");
				return;
			}
			
			JsonUtility.FromJsonOverwrite(_json.text, this);
			//todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
			// ответ: если в json нет Records то после десериализации останется null и потом будет NullReference, создаём пустой список
			if (Records == null)
				Records = new List<Data>(10);
		}

		private void OnDrawGizmos()
		{
			//todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
			// ответ: без них при пустом списке упадём на data[0] с выходом за границы
			if (Records == null || Records.Count == 0) return;
			var data = Records;
			var prev = data[0].Position;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(prev, 0.3f);
			//todo comment: Почему итерация начинается не с нулевого элемента?
			// ответ: нулевую точку уже взяли как prev до цикла, с неё и начинаем вести линии, иначе была бы линия сама в себя
			for (int i = 1; i < data.Count; i++)
			{
				var curr = data[i].Position;
				Gizmos.DrawWireSphere(curr, 0.3f);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}
		
#if UNITY_EDITOR
		[ContextMenu("Create File")]
		private void CreateFile()
		{
			//todo comment: Что происходит в этой строке?
			// ответ: создаём пустой файл Path.txt в папке Assets, dataPath это путь до Assets
			var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
			//todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав) 
			// ответ: закрывает хендл файла, иначе он висит занятый и Refresh его не подхватит
			stream.Dispose();
			UnityEditor.AssetDatabase.Refresh();
			//В Unity можно искать объекты по их типу, для этого используется префикс "t:"
			//После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
			var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
			foreach (var guid in guids)
			{
				//Этой командой можно получить путь к ассету через его гуид
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				//Этой командой можно загрузить сам ассет
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				//todo comment: Для чего нужны эти проверки?
				// ответ: FindAssets находит все текстовые ассеты, а нам нужен только наш Path, ну и на null проверяемся
				if(asset != null && asset.name == "Path")
				{
					_json = asset;
					UnityEditor.EditorUtility.SetDirty(this);
					UnityEditor.AssetDatabase.SaveAssets();
					UnityEditor.AssetDatabase.Refresh();
					//todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
					// ответ: уже нашли что надо и привязали, дальше перебирать смысла нет
					return;
				}
			}
		}

		private void OnDestroy()
		{
			if (_json == null) return;
			var text = JsonUtility.ToJson(this, true);
			var path = UnityEditor.AssetDatabase.GetAssetPath(_json);
			path = Path.Combine(Application.dataPath.Replace("Assets", ""), path);
			File.WriteAllText(path, text);
			UnityEditor.EditorUtility.SetDirty(_json);
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
		}
#endif
	}
}
