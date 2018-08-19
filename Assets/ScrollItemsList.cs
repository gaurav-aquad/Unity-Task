using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleHTTP;

public class ScrollItemsList : MonoBehaviour {

	public GameObject itemGO;
	public GameObject verticalScrollBarGO;
	[Range(1, 100)]
	public int itemsPerPage = 10;
	private Scrollbar verticalScrollBar;
	private string itemsURL = "http://takshila.stubuz.com/temp/get";
	private int pageNo = 1, pageLimit = 0;
	private bool isPageLimitReached = false;
	private GameObject[] itemsGO;

	// Use this for initialization
	void Start () {
		//Check the range
		itemsPerPage = (itemsPerPage <= 0) ? 10 : itemsPerPage;
		itemsPerPage = (itemsPerPage > 100) ? 100 : itemsPerPage;

		verticalScrollBar = verticalScrollBarGO.GetComponent<Scrollbar> ();

		//Instantiate required no. of objects
		itemsGO = new GameObject[itemsPerPage];
		for (int i = 0; i < itemsPerPage; i++) {
			itemsGO[i] = instantiateNewItem ();
			itemsGO[i].transform.SetParent (this.transform, false);
			itemsGO[i].GetComponent<RectTransform> ().sizeDelta = new Vector2 (1, 1);
		}
		fetchDetails ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
	}

	void adjustVerticalScroll() {
		verticalScrollBar.value = 1;
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
		Request request = new Request (itemsURL+"/"+pageNo+"/"+itemsPerPage);

		Client http = new Client ();
		yield return http.Send (request);
		ProcessResult (http);
	}

	void ProcessResult(Client http) {
		if (http.IsSuccessful ()) {
			Response resp = http.Response ();

			Item[] items = JsonHelper.getJsonArray<Item>(resp.Body());
			//If there are no items from the server
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

			updateView (items);
		}
		else {
			Debug.Log (""+http.Error());
		}
	}

	void updateView(Item[] items) {
		//Read all the items
		for(int i=0; i<items.Length; i++) {

			//If item isn't active, activate it
			if (!itemsGO [i].activeSelf)
				itemsGO [i].SetActive (true);

			//Set the name and price
			itemsGO[i].GetComponent<ItemScript>().initData(items[i].name, ""+items[i].price, items[i].image);

		}

		//Disable the items which don't have any data to show
		for(int i=items.Length; i<itemsPerPage; i++) {
			itemsGO [i].SetActive(false);
		}

		adjustVerticalScroll ();
	}

	GameObject instantiateNewItem() {
		GameObject prefab = (GameObject)Instantiate (itemGO);
		return prefab;
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
