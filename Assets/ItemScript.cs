using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemScript : MonoBehaviour {

	public Text nameLabel, priceLabel;
	public GameObject imageGO;
	private Image imageComponent;

	// Use this for initialization
	void Start () {
		imageComponent = imageGO.GetComponent<Image> ();
	}
	
	public void initData(string name, string price, string image) {
		nameLabel.text = name;
		priceLabel.text = price;

		imageComponent.sprite = null;
		StartCoroutine (FetchImageCoroutine(image));
	}

	IEnumerator FetchImageCoroutine(string url)
	{
		using (WWW www = new WWW(url))
		{
			yield return www;
			imageComponent.sprite = 
				Sprite.Create(www.texture, new Rect(0,0,www.texture.width,www.texture.height), new Vector2(0.5f, 0.5f));
		}
	}
}
