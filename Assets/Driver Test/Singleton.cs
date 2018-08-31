using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T instance;
	public static GameObject singletonObject;

	//Returns the instance of this singleton.
	public static T Instance
	{
		get
		{
			if(instance == null)
			{
				instance = (T) FindObjectOfType(typeof(T));

				if (instance == null)
				{
					Debug.LogError("An instance of " + typeof(T) + 
						" is needed in the scene, but there is none.");
				}
			}

			return instance;
		}
	}

	public static void InitializeSingleton () {
		if (singletonObject == null) {
			singletonObject = GameObject.Find ("Singletons");

			if (singletonObject == null) {
				singletonObject = new GameObject ("Singletons");
				DontDestroyOnLoad (singletonObject);

				if (singletonObject == null) {
					Debug.LogError ("The application could not initialize the singleton object.");
				}
			}
		}

		singletonObject.AddComponent<T> ();
		instance = (T) FindObjectOfType(typeof(T));
	}

}
