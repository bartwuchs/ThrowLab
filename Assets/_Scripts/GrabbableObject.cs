using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class GrabbableObject : MonoBehaviour {


    #region public variables
    public bool UseFixedUpdate = true;
    public float ReleaseMoment = 0.1f;
    public int nFramesPast = 3;
    public int nFramesFuture = 2;
    public VelocitySource VelocitySource;

    #endregion

    #region private variables & properties
    
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    SteamVR_Controller.Device _device;

    private bool _grabbed;
    private Vector3[] _recordedVelocities;
    private Vector3 _lastPosition;
    private Rigidbody _rigidbody;
    private int _framesSinceRelease;
    private bool _released;
    private Vector3 _orgPosition;
    private float timeStep { get { return UseFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime; } }

    #endregion

    #region initialization
    // Use this for initialization
    void Start () {
        _recordedVelocities = new Vector3[nFramesPast];
	    _rigidbody = GetComponent<Rigidbody>();
        _orgPosition = transform.position;
    }
    #endregion

    #region updates
    // Update is called once per frame
    void Update () {
        if (UseFixedUpdate)
            return;
	    MyUpdate();

	}

    void FixedUpdate()
    {
        if(!UseFixedUpdate)
            return;
        MyUpdate();
    }

    private void MyUpdate()
    {
        if (!_grabbed)
            return;

        RecordVelocity();
    
        if (ShouldRelease())
        {
            Release();
        }
        if (ShouldPostRelease())
        {
            DoPostRelease();
        }
        CheckFinished();
    }

    #endregion

    #region checks
    private void CheckFinished()
    {
        if (_released && _framesSinceRelease >= nFramesFuture)
        {
            _grabbed = false;
            _released = false;
            _framesSinceRelease = -1;
        }
    }

    private bool ShouldPostRelease()
    {
        _framesSinceRelease++;
        return (_released && _framesSinceRelease >= nFramesFuture);
    }

    private bool ShouldRelease()
    {
        return !_released && _device.GetAxis(triggerButton).x <= ReleaseMoment;
    }

    #endregion

    #region events & inputs 
    public void ForcedRelease()
    {
        DoPostRelease();
    }

    private void DoPostRelease()
    {
        ApplyVelocity();
        _grabbed = false;
    }

    public void Grabbed(SteamVR_Controller.Device device)
    {
        _rigidbody.isKinematic = true;
        _device = device;
        _grabbed = true;

    }

    public void Release()
    {
        _released = true;
        _framesSinceRelease = -1;
    }

    private void ApplyVelocity()
    {

        _rigidbody.isKinematic = false;
        transform.parent = null;

        _rigidbody.velocity = GetAveragePastVelocity();
    }

    

    #endregion

    #region record
    private void RecordVelocity()
    {
        switch (VelocitySource)
        {
                case VelocitySource.Controller:
                RecordVelocityDevice();
                return;

                case  VelocitySource.Object:
                RecordVelocitiesObject();
                return;
        }
    }
    private void RecordVelocityDevice()
    {
        for (int i = _recordedVelocities.Length - 1; i > 0; i--)
        {
            _recordedVelocities[i] = _recordedVelocities[i - 1];
        }
        _recordedVelocities[0] = _device.velocity;
    }

    private void RecordVelocitiesObject()
    {
        for (int i = _recordedVelocities.Length - 1; i > 0; i--)
        {
            _recordedVelocities[i] = _recordedVelocities[i - 1];
        }
        _recordedVelocities[0] = (transform.position - _lastPosition) / timeStep;
        _lastPosition = transform.position;
    }

    #endregion

    #region calculations

    private Vector3 GetAveragePastVelocity()
    {
        Vector3 sum = new Vector3();
        sum = _recordedVelocities.Aggregate(sum, (current, t) => current + t);
        return sum/_recordedVelocities.Length;
    }



    #endregion

    #region collisions
    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Ground")
        {
            StartCoroutine(Reset());
        }
    }

    private IEnumerator Reset()
    {
        transform.position = _orgPosition;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.isKinematic = true;
        yield return new WaitForEndOfFrame();
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.isKinematic = false;
    }

    #endregion
}

public enum VelocitySource
{
    Controller,
    Object
}
