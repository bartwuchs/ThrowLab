using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour {

    private SteamVR_TrackedController _controller;
    private GrabbableObject _selectedObject;
    public Transform GrabPoint;

    private SteamVR_Controller.Device Device    // The device property that is the controller, so that we can tell what index we are on
    {
        get { return SteamVR_Controller.Input((int) _controller.controllerIndex); }
    }

    #region Subscribe to events
    private void OnEnable()
    {
        _controller = GetComponent<SteamVR_TrackedController>();
        _controller.TriggerClicked += OnTriggerClicked;
       // _controller.TriggerUnclicked += OnTriggerUnClicked;
    }

    private void OnDisable()
    {
        _controller.TriggerClicked -= OnTriggerClicked;
      //  _controller.TriggerUnclicked -= OnTriggerUnClicked;
    }

    private void OnTriggerClicked(object sender, ClickedEventArgs e)
    {
        if (_selectedObject != null)
        {
            GrabAndSnap();
            _selectedObject.Grabbed(Device);
        }
    }

   

    #endregion
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region collision
    void OnTriggerStay(Collider other)
    {
        if (other.transform.GetComponent<GrabbableObject>() != null)
        {
            _selectedObject = other.transform.GetComponent<GrabbableObject>();
        }
    }

    private void GrabAndSnap()
    {
        _selectedObject.transform.position = GrabPoint.position;
        _selectedObject.transform.SetParent(transform,true);
    }

    #endregion
}
