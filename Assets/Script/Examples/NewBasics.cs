using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewBasics : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
        // use key R to rotate on y-axis
        if (Input.GetKey(KeyCode.R))
        {
            // rotate on y-axis by 1-degree
            transform.Rotate(Vector3.up, 1f);
        }

        // key up/down for forward/backward motion based on Forward vector for that GameObject
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(new Vector3(0, 0, 1) * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(new Vector3(0, 0, -1) * Time.deltaTime);
        }

        // key S to scale GameObject on z-axis
        // * Time.deltaTime to smoothen the scale
        if (Input.GetKey(KeyCode.S))
        {
            transform.localScale += new Vector3(0, 0, 1) * Time.deltaTime;
        }
	}

    // to determine if we clicking over the UI element or not
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (var result in results)
        {
            Debug.Log(result.gameObject.name);
        }

        return results.Count > 0;
    }
}
