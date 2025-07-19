using Terresquall;
using UnityEngine;
using Voidwalker;

namespace CarControllerwithShooting
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        [SerializeField] private GameObject[] _wheelMeshes = new GameObject[4];
        public WheelCollider[] wheelColliders = new WheelCollider[4];
        [SerializeField] private WheelEffects[] _wheelEffects = new WheelEffects[4];
        [SerializeField] private MeshRenderer _brakeLightMeshRenderer;
        [SerializeField] private MeshRenderer _reverseLightMeshRenderer;

        public GameObject ExplosionParticle;

        public static CarController Instance;
        public int Health = 100;

        [SerializeField] private Vector3 _centerOfMassOffset;
        [Range(15f, 30f)]
        [SerializeField] private float _maximumSteerAngle = 25f;
        [SerializeField] private float _fullTorqueOverAllWheels = 560f;
        [SerializeField] private float _reverseTorque = 1200f;
        [SerializeField] private float _maxHandbrakeTorque = 150f;
        [SerializeField] private float _topSpeed = 150f;
        [SerializeField] private float _revRangeBoundary = 1f;
        [Range(0.3f, 1f)]
        [SerializeField] private float _slipLimit = 0.6f;
        [SerializeField] private float _brakeTorque = 15000f;
        [SerializeField] private float _smoothInputSpeed = 0.15f;
        private static int NumberOfGears = 5;

        [SerializeField] private float _antiRollVal = 8000f;
        [SerializeField] private float _downForce = 200f;
        [SerializeField, Range(0, 1)] private float _steerHelper = 0.3f; 
        [SerializeField, Range(0, 1)] private float _tractionControl = 0.8f; 

        [SerializeField] private float _steeringSensitivity = 1f;
        [SerializeField] private float _maxSteerVelocity = 2f;
        [SerializeField] private AnimationCurve _speedSteerCurve = AnimationCurve.Linear(0, 1, 100, 0.3f);
        [SerializeField] private float _stabilityForce = 1000f;
        [SerializeField] private float _emergencyBrakeMultiplier = 2f;

        [SerializeField] private float _slowdownRate = 2f;
        private bool _stopCar = false;
        private float _slowdownFactor = 1f;

        [SerializeField] private VirtualJoystick virtualJoystick;

        private Quaternion[] _wheelMeshLocalRotations;
        private float _steerAngle;
        private float _targetSteerAngle;
        private int _gearNum;
        private float _gearFactor;
        private float _oldRotation;
        private float _currentTorque;
        private Rigidbody _rigidbody;
        private Vector2 _currentInputVector;
        private Vector2 _smoothInputVelocity;
        private int _emissionPropertyId;
        private float _currentMaxSteerAngle;
        private bool _playerPressedKey;

        public GameObject crushingParticle;

        public bool Skidding { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle { get { return _steerAngle; } }
        public float CurrentSpeed { get { return _rigidbody.linearVelocity.magnitude * 3.6f; } }
        public float MaxSpeed { get { return _topSpeed; } }
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        private void Awake()
        {
            Instance = this;
            _wheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                _wheelMeshLocalRotations[i] = _wheelMeshes[i].transform.localRotation;
            }

            _maxHandbrakeTorque = float.MaxValue;
            _rigidbody = GetComponent<Rigidbody>();
            _currentTorque = _fullTorqueOverAllWheels - (_tractionControl * _fullTorqueOverAllWheels);
            _rigidbody.centerOfMass += _centerOfMassOffset;
            _emissionPropertyId = Shader.PropertyToID("_EmissionColor");

            SetupWheelColliders();
        }

        private void SetupWheelColliders()
        {
            foreach (var wheel in wheelColliders)
            {
                WheelFrictionCurve forwardFriction = wheel.forwardFriction;
                WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

                forwardFriction.extremumSlip = 0.4f;
                forwardFriction.extremumValue = 1.2f;
                forwardFriction.asymptoteSlip = 0.8f;
                forwardFriction.asymptoteValue = 1.0f;
                forwardFriction.stiffness = 2f;

                sidewaysFriction.extremumSlip = 0.25f;
                sidewaysFriction.extremumValue = 1.3f;
                sidewaysFriction.asymptoteSlip = 0.5f;
                sidewaysFriction.asymptoteValue = 1.1f;
                sidewaysFriction.stiffness = 2.5f;

                wheel.forwardFriction = forwardFriction;
                wheel.sidewaysFriction = sidewaysFriction;

                JointSpring suspensionSpring = wheel.suspensionSpring;
                suspensionSpring.spring = 35000f;
                suspensionSpring.damper = 4500f;
                suspensionSpring.targetPosition = 0.5f;
                wheel.suspensionSpring = suspensionSpring;

                wheel.suspensionDistance = 0.3f;
                wheel.forceAppPointDistance = 0f;
            }
        }

        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
            float upGearLimit = (1 / (float)NumberOfGears) * (_gearNum + 1);
            float downGearLimit = (1 / (float)NumberOfGears) * _gearNum;

            if (_gearNum > 0 && f < downGearLimit)
                _gearNum--;

            if (f > upGearLimit && (_gearNum < (NumberOfGears - 1)))
                _gearNum++;
        }

        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }

        private static float UnclampedLerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }

        float LastParticleTime = 0;
        private void OnCollisionStay(Collision collision)
        {
            if (Time.time > LastParticleTime + 0.1f && Mathf.Abs(collision.relativeVelocity.magnitude) > 1)
            {
                LastParticleTime = Time.time;
                Instantiate(crushingParticle, collision.contacts[0].point, Quaternion.identity);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Collapsable"))
            {
                Rigidbody rigidbody = collision.collider.GetComponent<Rigidbody>();
                if (rigidbody != null && rigidbody.isKinematic)
                {
                    rigidbody.isKinematic = false;
                }
            }
        }

        private void CalculateGearFactor()
        {
            float f = (1 / (float)NumberOfGears);
            float targetGearFactor = Mathf.InverseLerp(f * _gearNum, f * (_gearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
            _gearFactor = Mathf.Lerp(_gearFactor, targetGearFactor, Time.deltaTime * 5.0f);
        }

        private void CalculateRevs()
        {
            CalculateGearFactor();
            float gearNumFactor = _gearNum / (float)NumberOfGears;
            float revsRangeMin = UnclampedLerp(0f, _revRangeBoundary, CurveFactor(gearNumFactor));
            float revsRangeMax = UnclampedLerp(_revRangeBoundary, 1f, gearNumFactor);
            Revs = UnclampedLerp(revsRangeMin, revsRangeMax, _gearFactor);
        }

        public void GetDamage(int Damage)
        {
            if (Health <= 0) return;

            Health = Health - Damage;
            if (Health <= 0)
            {
                Health = 0;
                ExplodeCar();
                ShowGameOverUI();
            }
            GameCanvas.Instance.Update_Text_Health();
        }

        private static void ShowGameOverUI()
        {
            GameCanvas.Instance.Show_GameOver();
            GameCanvas.Instance.GasolineUI.SetActive(false);
        }

        private static void ShowWinUI()
        {
            GameCanvas.Instance.Show_WinScreen();
            GameCanvas.Instance.GasolineUI.SetActive(false);
        }

        void ExplodeCar()
        {
            if (CarSystemManager.Instance.cameraFPS.activeSelf)
            {
                CarSystemManager.Instance.cameraFPS.SetActive(false);
                CarSystemManager.Instance.cameraTPS.SetActive(true);
            }
            Instantiate(ExplosionParticle, transform.position, Quaternion.identity);
            Destroy(gameObject, 0.2f);
            Debug.Log("Exploded!");
            GameCanvas.Instance.Hide_GameUI();
        }

        Vector2 input;
        float footBrake;

        [HideInInspector]
        public float handBrake;

        private void Update()
        {
            // Check if we've just run out of gas
            if (!_stopCar && Gasoline.Instance.CurrentFuel <= 0)
            {
                _stopCar = true;
            }
        }

        public void Move()
        {
            if (_stopCar && Gasoline.Instance.CurrentFuel <= 0)
            {
                StopCarSmoothly();
                ShowGameOverUI();
                return;
            }
            else if (_stopCar)
            {
                StopCarSmoothly();
                ShowWinUI();
                return;
            }

            if (CarSystemManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                footBrake = Input.GetAxis("Vertical");
            }
            else
            {
                input = new Vector2(virtualJoystick.GetAxis("Horizontal"), virtualJoystick.GetAxis("Vertical"));
                footBrake = virtualJoystick.GetAxis("Vertical");
            }

            _currentInputVector = Vector2.SmoothDamp(_currentInputVector, input, ref _smoothInputVelocity, _smoothInputSpeed);
            float accel = _currentInputVector.y;
            float steering = _currentInputVector.x;

            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion quat);
                _wheelMeshes[i].transform.SetPositionAndRotation(position, quat);
            }

            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footBrake = -1 * Mathf.Clamp(footBrake, -1, 0);
            handBrake = Mathf.Clamp(handBrake, 0, 1);

            ApplySteering(steering);
            SteerHelper();
            ApplyDrive(accel, footBrake);
            ApplyStabilityForces();
            CapSpeed();

            if (handBrake > 0f)
            {
                float handBrakeTorque = handBrake * _maxHandbrakeTorque;
                wheelColliders[2].brakeTorque = handBrakeTorque;
                wheelColliders[3].brakeTorque = handBrakeTorque;
                TurnBrakeLightsOn();
            }
            else
            {
                wheelColliders[2].brakeTorque = 0f;
                wheelColliders[3].brakeTorque = 0f;
            }

            CalculateRevs();
            GearChanging();
            AddDownForce();
            CheckForWheelSpin();
            TractionControl();
            AntiRoll();
            UpdateSteerAngle();
        }

        private void ApplySteering(float steeringInput)
        {
            float speedFactor = _speedSteerCurve.Evaluate(CurrentSpeed);
            float sensitivityAdjustedInput = steeringInput * _steeringSensitivity * speedFactor;

            _targetSteerAngle = sensitivityAdjustedInput * _currentMaxSteerAngle;

            // Smooth steering interpolation to prevent jerky movements
            _steerAngle = Mathf.MoveTowards(_steerAngle, _targetSteerAngle, _maxSteerVelocity * Time.deltaTime * _currentMaxSteerAngle);

            wheelColliders[0].steerAngle = _steerAngle;
            wheelColliders[1].steerAngle = _steerAngle;
        }

        private void ApplyStabilityForces()
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            float sideSlip = Vector3.Dot(velocity, right);

            if (Mathf.Abs(sideSlip) > 0.5f)
            {
                Vector3 counterForce = -right * sideSlip * _stabilityForce * Time.deltaTime;
                _rigidbody.AddForce(counterForce);
            }

            Vector3 angularVel = _rigidbody.angularVelocity;
            if (angularVel.magnitude > 3f)
            {
                _rigidbody.angularVelocity = angularVel.normalized * 3f;
            }
        }

        private void StopCarSmoothly()
        {
            if (CurrentSpeed > 0.1f)
            {
                float emergencyBrakeTorque = _brakeTorque * _emergencyBrakeMultiplier;
                for (int i = 0; i < 4; i++)
                {
                    wheelColliders[i].motorTorque = 0;
                    wheelColliders[i].brakeTorque = emergencyBrakeTorque;
                }

                TurnBrakeLightsOn();
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    wheelColliders[i].motorTorque = 0;
                    wheelColliders[i].brakeTorque = _brakeTorque * 3f;
                }

                TurnBrakeLightsOff();

                if (_rigidbody.linearVelocity.magnitude < 0.1f)
                {
                    _rigidbody.linearVelocity = Vector3.zero;
                    _rigidbody.angularVelocity = Vector3.zero;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].GetWorldPose(out Vector3 position, out Quaternion quat);
                _wheelMeshes[i].transform.SetPositionAndRotation(position, quat);
            }

            GameCanvas.Instance.Update_Text_Speed();
        }

        public float speed;
        private void CapSpeed()
        {
            speed = _rigidbody.linearVelocity.magnitude;
            speed *= 3.6f;
            if (speed > _topSpeed)
                _rigidbody.linearVelocity = (_topSpeed / 3.6f) * _rigidbody.linearVelocity.normalized;

            GameCanvas.Instance.Update_Text_Speed();
        }

        private void ApplyDrive(float accel, float footBrake)
        {
            float thrustTorque = accel * (_currentTorque / 4f);

            if (Gasoline.Instance.CurrentFuel > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    wheelColliders[i].motorTorque = thrustTorque;
                }

                Gasoline.Instance.CurrentFuel = Gasoline.Instance.CurrentFuel - thrustTorque * Time.deltaTime * Gasoline.Instance.FuelConsumptionRate;

                if (Gasoline.Instance.CurrentFuel < 0)
                {
                    Gasoline.Instance.CurrentFuel = 0;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    wheelColliders[i].motorTorque = 0;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, _rigidbody.linearVelocity) < 50f)
                {
                    wheelColliders[i].brakeTorque = _brakeTorque * footBrake;
                }
                else if (footBrake > 0)
                {
                    wheelColliders[i].brakeTorque = 0f;
                    wheelColliders[i].motorTorque = -_reverseTorque * footBrake;
                }
            }

            if (footBrake > 0)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, _rigidbody.linearVelocity) < 50f)
                {
                    TurnBrakeLightsOn();
                }
                else
                {
                    TurnBrakeLightsOff();
                    TurnReverseLightsOn();
                }
            }
            else
            {
                TurnBrakeLightsOff();
                TurnReverseLightsOff();
            }
        }

        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].GetGroundHit(out WheelHit wheelHit);
                if (wheelHit.normal == Vector3.zero)
                    return;
            }

            if (Mathf.Abs(_oldRotation - transform.eulerAngles.y) < 10f)
            {
                float turnAdjust = (transform.eulerAngles.y - _oldRotation) * _steerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
                _rigidbody.linearVelocity = velRotation * _rigidbody.linearVelocity;
            }

            _oldRotation = transform.eulerAngles.y;
        }

        private void AntiRoll()
        {
            float travelL = 1.0f;
            float travelR = 1.0f;
            bool groundedLf = wheelColliders[0].GetGroundHit(out WheelHit wheelHit);

            if (groundedLf)
                travelL = (-wheelColliders[0].transform.InverseTransformPoint(wheelHit.point).y - wheelColliders[0].radius) / wheelColliders[0].suspensionDistance;

            bool groundedRf = wheelColliders[1].GetGroundHit(out wheelHit);

            if (groundedRf)
                travelR = (-wheelColliders[1].transform.InverseTransformPoint(wheelHit.point).y - wheelColliders[1].radius) / wheelColliders[1].suspensionDistance;

            float antiRollForce = (travelL - travelR) * _antiRollVal;

            if (groundedLf)
                _rigidbody.AddForceAtPosition(wheelColliders[0].transform.up * -antiRollForce, wheelColliders[0].transform.position);

            if (groundedRf)
                _rigidbody.AddForceAtPosition(wheelColliders[1].transform.up * antiRollForce, wheelColliders[1].transform.position);

            bool groundedLr = wheelColliders[2].GetGroundHit(out wheelHit);

            if (groundedLr)
                travelL = (-wheelColliders[2].transform.InverseTransformPoint(wheelHit.point).y - wheelColliders[2].radius) / wheelColliders[2].suspensionDistance;

            bool groundedRr = wheelColliders[3].GetGroundHit(out wheelHit);

            if (groundedRr)
                travelR = (-wheelColliders[3].transform.InverseTransformPoint(wheelHit.point).y - wheelColliders[3].radius) / wheelColliders[3].suspensionDistance;

            antiRollForce = (travelL - travelR) * _antiRollVal;

            if (groundedLr)
                _rigidbody.AddForceAtPosition(wheelColliders[2].transform.up * -antiRollForce, wheelColliders[2].transform.position);

            if (groundedRr)
                _rigidbody.AddForceAtPosition(wheelColliders[3].transform.up * antiRollForce, wheelColliders[3].transform.position);
        }

        private void AddDownForce()
        {
            if (_downForce > 0)
                _rigidbody.AddForce(_downForce * _rigidbody.linearVelocity.magnitude * -transform.up);
        }

        private void CheckForWheelSpin()
        {
            Skidding = false;
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].GetGroundHit(out WheelHit wheelHit);

                if (Mathf.Abs(wheelHit.forwardSlip) >= _slipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= _slipLimit)
                {
                    Skidding = true;
                    _wheelEffects[i].EmitTireSmoke();

                    if (!AnySkidSoundPlaying())
                    {
                        _wheelEffects[i].PlayAudio();
                    }
                    continue;
                }

                if (_wheelEffects[i].IsPlayingAudio)
                    _wheelEffects[i].StopAudio();

                _wheelEffects[i].EndSkidTrail();
            }
        }

        void TractionControl()
        {
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].GetGroundHit(out WheelHit wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
            }
        }

        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= _slipLimit && _currentTorque >= 0)
            {
                _currentTorque -= 15 * _tractionControl;
            }
            else
            {
                _currentTorque += 15 * _tractionControl;
                if (_currentTorque > _fullTorqueOverAllWheels)
                {
                    _currentTorque = _fullTorqueOverAllWheels;
                }
            }
        }

        private bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_wheelEffects[i].IsPlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }

        private void TurnBrakeLightsOn()
        {
            _brakeLightMeshRenderer.material.SetColor(_emissionPropertyId, Color.red);

            if (!_brakeLightMeshRenderer.material.IsKeywordEnabled("_EMISSION"))
                _brakeLightMeshRenderer.material.EnableKeyword("_EMISSION");
        }

        private void TurnBrakeLightsOff()
        {
            _brakeLightMeshRenderer.material.SetColor(_emissionPropertyId, Color.black);
        }

        private void TurnReverseLightsOn()
        {
            _reverseLightMeshRenderer.material.SetColor(_emissionPropertyId, Color.white);

            if (!_reverseLightMeshRenderer.material.IsKeywordEnabled("_EMISSION"))
                _reverseLightMeshRenderer.material.EnableKeyword("_EMISSION");
        }

        private void TurnReverseLightsOff()
        {
            _reverseLightMeshRenderer.material.SetColor(_emissionPropertyId, Color.black);
        }

        private void UpdateSteerAngle()
        {
            float speedBasedReduction = CurrentSpeed / 100f;

            if (CurrentSpeed < 25f)
            {
                _currentMaxSteerAngle = Mathf.MoveTowards(_currentMaxSteerAngle, _maximumSteerAngle, 1f);
            }
            else if (CurrentSpeed < 60f)
            {
                float targetAngle = _maximumSteerAngle * (1f - speedBasedReduction * 0.3f);
                _currentMaxSteerAngle = Mathf.MoveTowards(_currentMaxSteerAngle, targetAngle, 1f);
            }
            else
            {
                float targetAngle = _maximumSteerAngle * (1f - speedBasedReduction * 0.5f);
                _currentMaxSteerAngle = Mathf.MoveTowards(_currentMaxSteerAngle, targetAngle, 1f);
            }
        }

        public void SetCarStop(bool value)
        {
            _stopCar = value;
        }
    }
}