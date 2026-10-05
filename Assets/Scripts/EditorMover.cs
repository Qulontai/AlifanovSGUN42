using UnityEngine;

namespace DefaultNamespace
{
	
	[RequireComponent(typeof(PositionSaver))]
	public class EditorMover : MonoBehaviour
	{
		private PositionSaver _save;
		private float _currentDelay;
		
		//todo comment: Что произойдёт, если _delay > _duration?
		// ответ: таймер _duration кончится раньше первой записи и точек вообще не будет, список останется пустой
		[SerializeField, Range(0.2f, 1.0f)]
		private float _delay = 0.5f;
		[SerializeField, Min(0.2f)]
		private float _duration = 5f;

		private void Start()
		{
			//todo comment: Почему этот поиск производится здесь, а не в начале метода Update?
			// ответ: GetComponent каждый кадр дёргать накладно, хватает один раз закэшировать в Start
			_save = GetComponent<PositionSaver>();
			if (_duration <= _delay)
				_duration = _delay * 5f;
			_save.Records.Clear();
		}

		private void Update()
		{
			_duration -= Time.deltaTime;
			if (_duration <= 0f)
			{
				enabled = false;
				Debug.Log($"<b>{name}</b> finished", this);
				return;
			}
			
			//todo comment: Почему не написать (_delay -= Time.deltaTime;) по аналогии с полем _duration?
			// ответ: _delay это настройка, её портить нельзя, поэтому тикаем отдельную _currentDelay а _delay только для сброса
			_currentDelay -= Time.deltaTime;
			if (_currentDelay <= 0f)
			{
				_currentDelay = _delay;
				_save.Records.Add(new PositionSaver.Data
				{
					Position = transform.position,
					//todo comment: Для чего сохраняется значение игрового времени?
					// ответ: чтобы реплей знал когда какая точка была и ехал в том же темпе через Lerp
					Time = Time.time,
				});
			}
		}
	}
}
