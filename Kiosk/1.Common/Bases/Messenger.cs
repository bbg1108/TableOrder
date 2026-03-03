using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk
{
	//public class Messenger
	//{
	//	private readonly Dictionary<MessengerEnum, Action<object>> _actions;
	//	private readonly List<Tuple<Delegate, Action<object>>> _actionTuples;
	//	private static Messenger _instance;
	//	private static readonly object _lock = new object();

	//	//Singletone Instance
	//	public static Messenger Instance 
	//	{	get 
	//		{	lock (_lock) 
	//			{
	//				if (_instance == null)
	//				{
	//					_instance = new Messenger();
	//				}
	//				return _instance;
	//			}
	//		} 
	//	}

	//	//Singletone 외부에서 생성이 안되도록
	//	private Messenger()
	//	{
	//		//실제로 Subscribe된 Delegate를 실행하는 곳.
	//		_actions = new Dictionary<MessengerEnum, Action<object>>();
	//		//Subscribe한 Delegate를 Action<object> 형식으로 만들어서 가지고 있음.
	//		_actionTuples = new List<Tuple<Delegate, Action<object>>>();
	//	}

	//	public void Subscribe<T>(MessengerEnum key, Action<T> action)
	//	{
	//		Action<object> input = null;
	//		input = _actionTuples.Find(x => x.Item1.Equals(action))?.Item2;
	//		if (input == null)
	//		{
	//			//Action<T>를 Action<object> 형태로 변형해서 List에 추가.
	//			input = new Action<object>(o => action((T)o));
	//			_actionTuples.Add(new Tuple<Delegate, Action<object>>(action, input));
	//		}
	//		else
	//		{
	//			return;
	//		}
	//		//Action<T>가 아닌 Action<object>를 Dctionary에 넣어준다.
	//		if (!_actions.ContainsKey(key))
	//		{
	//			_actions.Add(key, input);
	//		}
	//		else
	//		{
	//			_actions[key] += input;
	//		}
	//	}

	//	public void UnSubscribe<T>(MessengerEnum key, Action<T> action)
	//	{
	//		//Action<T>로 Action<object>를 찾고 Dictionary에서 제거
	//		Action<object> input = null;
	//		input = _actionTuples.Find(x => x.Item1.Equals(action))?.Item2;
	//		if (input == null)
	//		{
	//			return;
	//		}
	//		else
	//		{
	//			_actions[key] -= input;
	//		}
	//	}

	//	public void Send<T>(MessengerEnum key, T obj)
	//	{
	//		if (!_actions.ContainsKey(key))
	//		{
	//			return;
	//		}
	//		_actions[key]?.Invoke(obj);
	//	}
	//}

	/// <summary>
	/// 20260109 약한참조 메신저 추가
	/// </summary>
	public class Messenger
	{
		private readonly Dictionary<MessengerEnum, List<object>> _map
			= new Dictionary<MessengerEnum, List<object>>();
		private static Messenger _instance;
		private static readonly object _lock = new object();

		//Singletone Instance
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

		public void Subscribe<T>(MessengerEnum key, object target, Action<T> action)
		{
			if (!_map.ContainsKey(key))
				_map[key] = new List<object>();

			// 메신저의 동일한 함수 등록시 기존 함수 제거하고 새 함수 등록
			if (_map[key]?.Count > 0)
			{
				var oldAction = _map[key].First(x => (x as WeakAction<T>).MetadataToken == action.Method.MetadataToken);
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
