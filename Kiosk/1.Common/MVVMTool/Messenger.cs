using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk
{
	public class Messenger
	{
		private readonly Dictionary<MessengerEnum, List<object>> _map;
		private static Messenger _instance;
		private static readonly object _lock = new object();

		public static Messenger Instance
		{
			get
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = new Messenger();
					}
					return _instance;
				}
			}
		}

		private Messenger()
        {
			_map = new Dictionary<MessengerEnum, List<object>>();
		}

		public void Subscribe<T>(MessengerEnum key, object target, Action<T> action)
		{
			if (!_map.ContainsKey(key))
				_map[key] = new List<object>();

			// 동일한 함수 등록시 기존 함수 제거하고 새 함수 등록
			if (_map[key]?.Count > 0)
			{
				var oldAction = _map[key].FirstOrDefault(x => (x as WeakAction<T>)?.MetadataToken == action.Method.MetadataToken);
				if (oldAction != null)
					_map[key].Remove(oldAction);
			}

			_map[key].Add(new WeakAction<T>(target, action));
		}

		public void Send<T>(MessengerEnum key, T message)
		{
			if (!_map.ContainsKey(key))
				return;

			var list = _map[key];
			for (int i = list.Count - 1; i >= 0; i--)
			{
				var weakAction = list[i] as WeakAction<T>;
				if (weakAction == null) continue;

				if (!weakAction.IsAlive)
				{
					list.RemoveAt(i); // GC된 대상 정리
				}
				else
				{
					weakAction.Invoke(message);
				}
			}
		}
	}

	public class WeakAction<T>
	{
		public bool IsAlive => _targetRef.IsAlive;
		public int MetadataToken => _method.MetadataToken;  // 함수 식별자
		private readonly WeakReference _targetRef;
		private readonly MethodInfo _method;

		public WeakAction(object target, Action<T> action)
		{
			_targetRef = new WeakReference(target);
			_method = action.Method;
		}

		public void Invoke(T message)
		{
			var target = _targetRef.Target;
			if (target != null)
			{
				_method.Invoke(target, new object[] { message });
			}
		}
	}
}
