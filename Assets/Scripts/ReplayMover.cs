using System;
using UnityEngine;

namespace DefaultNamespace
{
	[RequireComponent(typeof(PositionSaver))]
	public class ReplayMover : MonoBehaviour
	{
		private PositionSaver _save;

		private int _index;
		private PositionSaver.Data _prev;
		private float _duration;

		private void Start()
		{
			////todo comment: зачем нужны эти проверки?
			// ответ: если сейва нет или список пустой то играть нечего, дальше будет выход за границы
			if (!TryGetComponent(out _save) || _save.Records.Count == 0)
			{
				Debug.LogError("Records incorrect value", this);
				//todo comment: Для чего выключается этот компонент?
				// ответ: чтоб Update больше не дёргался и не сыпал ошибками каждый кадр
				enabled = false;
			}
		}

		private void Update()
		{
			var curr = _save.Records[_index];
			//todo comment: Что проверяет это условие (с какой целью)? 
			// ответ: ждём пока время дойдёт до времени точки, так сохраняем паузы как при записи
			if (Time.time > curr.Time)
			{
				_prev = curr;
				_index++;
				//todo comment: Для чего нужна эта проверка?
				// ответ: проверяем что дошли до конца списка, выключаемся чтоб не вылететь за пределы
				if (_index >= _save.Records.Count)
				{
					enabled = false;
					Debug.Log($"<b>{name}</b> finished", this);
				}
			}
			//todo comment: Для чего производятся эти вычисления (как в дальнейшем они применяются)?
			// ответ: считаем сколько прошли между prev и curr по времени, это t для Lerp ниже
			var delta = (Time.time - _prev.Time) / (curr.Time - _prev.Time);
			//todo comment: Зачем нужна эта проверка?
			// ответ: если две точки в одно время то делим на ноль и будет NaN, сбрасываем в 0 чтоб не улететь
			if (float.IsNaN(delta)) delta = 0f;
			//todo comment: Опишите, что происходит в этой строчке так подробно, насколько это возможно
			// ответ: лерпим позицию между prev и curr по delta и двигаем туда кубик, за счёт этого едет плавно
			transform.position = Vector3.Lerp(_prev.Position, curr.Position, delta);
		}
	}
}
