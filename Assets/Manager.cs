using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleHTTP;

namespace Gaurav {
	
	public class Manager : MonoBehaviour {

		public GameObject[] itemsGO;
		private string itemsURL = "http://takshila.stubuz.com/temp/get";
		private int pageNo = 1, pageLimit = 0;
		bool isPageLimitReached = false;

		// Use this for initialization
		void Start () {
			fetchDetails ();
		}

		// Update is called once per frame
		void Update () {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				Application.Quit ();
			}
		}

		public void requestNextPage() {
			//If this is the last page for all the data available
			if (isPageLimitReached && pageNo == pageLimit)
				return;
			
			//Fetch next page items
			pageNo++;
			fetchDetails ();
		}

		public void requestPreviousPage() {
			//Can't go back, this is page 1
			if (pageNo == 1)
				return;
			
			//Fetch previous page items
			pageNo--;
			fetchDetails ();
		}

		public void fetchDetails() {
			StartCoroutine(GetDetailsCoroutine());
		}

		IEnumerator GetDetailsCoroutine() {
			Request request = new Request (itemsURL+"/"+pageNo);

			Client http = new Client ();
			yield return http.Send (request);
			ProcessResult (http);
		}

		void ProcessResult(Client http) {
			if (http.IsSuccessful ()) {
				Response resp = http.Response ();

				Item[] items = JsonHelper.getJsonArray<Item>(resp.Body());
				//No items from the server
				if (items.Length == 0) {
					isPageLimitReached = true;
					pageNo--;
					/*
					 * Just for this example project where we know we have fixed amount of rows in the table,
					 * we don't have to request this page again.
					 */
					pageLimit = pageNo;
					Debug.Log ("No items available.");
					return;
				}

				//Read all the items
				for(int i=0; i<items.Length; i++) {

					//If item isn't active, activate it
					if (!itemsGO [i].activeSelf)
						itemsGO [i].SetActive (true);

					//Set the name and price
					itemsGO [i].gameObject.GetComponent<Transform> ().GetChild (1).GetComponent<Text> ().text = items[i].name;
					itemsGO [i].gameObject.GetComponent<Transform> ().GetChild (2).GetComponent<Text> ().text = "Rs. "+items[i].price;

					//Set the image to none
					itemsGO [i].gameObject.GetComponent<Transform> ().GetChild (0).GetComponent<Image> ().sprite = null;
					//Fetch and set image for this item
					StartCoroutine (FetchImageCoroutine(items[i].image, itemsGO[i].gameObject));

				}

				//Disable the items which don't have any data to show
				for(int i=items.Length; i<10; i++) {
					itemsGO [i].SetActive(false);
				}

			}
			else {
				Debug.Log (""+http.Error());
			}
		}

		IEnumerator FetchImageCoroutine(string url, GameObject gameObj)
		{
			using (WWW www = new WWW(url))
			{
				yield return www;
				gameObj.GetComponent<Transform> ().GetChild (0).GetComponent<Image> ().sprite = Sprite.Create(www.texture, new Rect(0,0,www.texture.width,www.texture.height), new Vector2(0.5f, 0.5f));
			}
		}
	}

	public class JsonHelper
	 {
		 //Usage:
		 //YouObject[] objects = JsonHelper.getJsonArray<YouObject> (jsonString);
		 public static T[] getJsonArray<T>(string json)
		 {
			 string newJson = "{ \"array\": " + json + "}";
			 Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
			 return wrapper.array;
		 }
	 
		 //Usage:
		 //string jsonString = JsonHelper.arrayToJson<YouObject>(objects);
		 public static string arrayToJson<T>(T[] array)
		 {
			 Wrapper<T> wrapper = new Wrapper<T>();
			 wrapper.array = array;
			 return JsonUtility.ToJson(wrapper);
		 }
	 
		 [System.Serializable]
		 private class Wrapper<T>
		 {
			 public T[] array;
		 }
	 }

	[System.Serializable]
	public class Item
	{
		public int id, price;
		public string name, image;
	}
}
