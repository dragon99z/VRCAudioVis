using UnityEngine;
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC.SDKBase;
#endif
using System;

namespace AudioVisualizer
{
#if UDON
    using UdonSharp;
    using VRC.Udon;
    public class AudioVis : UdonSharpBehaviour
    {
#if AUDIOLINK
        [Header("AudioLink Settings")]
        public VRCAudioLink.AudioLink audioLinkInstance;
        public bool useAudioLinkColorTheme;
        [Range(0, 3)] public int AudioLinkColorTheme;
#endif
        [Header("Visualizer Settings")]
        public AudioSource audioSource;
        public GameObject[] audioSpectrumObjects;
        [Range(1, 100)] public float heightMultiplier = 1;
        [Range(64, 8192)] public int numberOfSamples = 1024; //step by 2
        public FFTWindow fftWindow;
        [Min(0f)] public float lerpTime = 1;

        [Header("Light Visualizer")]
        [Header("Light Intensity")]
        public bool intensityToggle;
        public int lightIntensity;
        [Header("Light Hue")]//HOE
        public bool HueToggle;
        [SerializeField] [Range(0f, 1f)] public float colorLerpTime = 0;
        private Color[] startColor;
        public Color[] lerpColor;
        [Range(0, 0.05f)] public float shiftSpeed = 0f;
        [SerializeField] [Range(0f, 1f)] public float colorShiftTime = 0;
        private bool isShifting = false;

        [Header("Particle Visualizer")]
        public bool burstToggle;
        public int burstIntensity;
        [Range(0, 10)] public float burstTime = 0f;
        [Min(0)]public int minBurst = 0;
        [Min(1)]public int maxBurst = 5;
        [Header("Transform Visualizer")]
        public bool transformToggle;

        [Header("Transform Position Settings")]
        public bool posToggle;
        public bool posX;
        public bool posY;
        public bool posZ;

        [Header("Transform Rotation Settings")]
        public bool rotateToggle;
        public bool rotateX;
        public bool rotateY;
        public bool rotateZ;
        public bool rotateW;

        [Header("Transform Scale Settings")]
        public bool scaleToggle;
        public bool scaleX;
        public bool scaleY;
        public bool scaleZ;
        public int scaleAddX = 0;
        public int scaleAddY = 0;
        public int scaleAddZ = 0;

        [Header("Orbit Settings")]
        public bool orbitToggle;
        public GameObject orbitCenterObject;
        public bool orbitVisualizer;
        [Range(1, 100)] public int orbitVisualizerSpeedMultiplyer = 10;
        public float orbitSpeed;

        public float orbitX;
        public float orbitY;
        public float orbitZ;

    private Color HueShift(Color color, float hueShiftAmount, float lTime)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            h += hueShiftAmount;
            Color output = Color.HSVToRGB(h, s, v, true);
            Color Lerp = Color.Lerp(color, output, lTime);
            float h1, s1, v1;
            Color.RGBToHSV(Lerp, out h1, out s1, out v1);
            return Color.HSVToRGB(h1, s, v,true);
        }

        void Start()
        {
#if AUDIOLINK
            if(audioLinkInstance != null)
            {

                if (audioLinkInstance.audioSource != null)
                {
                    audioSource = audioLinkInstance.audioSource;
                }

                if (useAudioLinkColorTheme)
                {
                    lerpColor = new Color[audioSpectrumObjects.Length];
                    for (int i = 0; i < audioSpectrumObjects.Length; i++)
                    {
                        switch (AudioLinkColorTheme)
                        {
                            case 0:
                                {
                                    lerpColor[i] = audioLinkInstance.customThemeColor0;
                                }
                                break;
                            case 1:
                                {
                                    lerpColor[i] = audioLinkInstance.customThemeColor1;
                                }
                                break;
                            case 2:
                                {
                                    lerpColor[i] = audioLinkInstance.customThemeColor2;
                                }
                                break;
                            case 3:
                                {
                                    lerpColor[i] = audioLinkInstance.customThemeColor3;
                                }
                                break;
                            default:
                                {
                                    lerpColor[i] = audioLinkInstance.customThemeColor0;
                                }
                                break;
                        }
                        
                    }
                }
            }
#endif
            startColor = new Color[audioSpectrumObjects.Length];
            for (int i = 0; i < audioSpectrumObjects.Length; i++)
            {
                Light SpectrumLight = audioSpectrumObjects[i].GetComponent<Light>();
                if (SpectrumLight != null)
                    startColor[i] = SpectrumLight.color;
            }
            if (HueToggle && shiftSpeed == 0f && lerpColor.Length != audioSpectrumObjects.Length)
            {
                lerpColor = new Color[audioSpectrumObjects.Length];
            }
        }

        void Update()
        {
#if AUDIOLINK
            if (audioLinkInstance != null)
            {
                if (audioLinkInstance.audioSource != null)
                {
                    audioSource = audioLinkInstance.audioSource;
                }
            }
#endif

            if (audioSource != null)
            {
            #region audioVis
                float[] spectrum = new float[numberOfSamples];

                audioSource.GetSpectrumData(spectrum, 0, fftWindow);

                for (int i = 0; i < audioSpectrumObjects.Length; i++)
                {
            #region LightVis
                    Light SpectrumLight = audioSpectrumObjects[i].GetComponent<Light>();
                    if (SpectrumLight != null)
                    {
                        float intensity = spectrum[i] * heightMultiplier;

                        if (intensityToggle)
                        {
                            SpectrumLight.intensity = Mathf.Lerp(SpectrumLight.intensity, intensity, lerpTime) * lightIntensity;
                        }

                        if (HueToggle)
                        {
                            if (SpectrumLight.color.r == 0 && SpectrumLight.color.g == 0 && SpectrumLight.color.b == 0)
                            {
                                SpectrumLight.color = new Vector4(0.5f, 0.75f, 0.25f, 1f);
                            }
                            else
                            {
                                if(shiftSpeed == 0f)
                                {
                                    if (SpectrumLight.color == startColor[i])
                                    {
                                        isShifting = true;
                                    }
                                    else if (SpectrumLight.color == lerpColor[i])
                                    {
                                        isShifting = false;
                                    }

                                    if (isShifting)
                                    {
                                        SpectrumLight.color = Color.Lerp(SpectrumLight.color, lerpColor[i], colorLerpTime);
                                    }
                                    else
                                    {
                                        SpectrumLight.color = Color.Lerp(SpectrumLight.color, startColor[i], colorLerpTime);
                                    }
                                }
                                else
                                {
                                    Color gray = new Vector4(0.494f, 0.494f, 0.494f, 1f);
                                    if (SpectrumLight.color.Equals(gray))
                                    {
                                        SpectrumLight.color = Color.red;
                                    }
                                    else
                                    {
                                        SpectrumLight.color = HueShift(SpectrumLight.color, shiftSpeed, colorShiftTime);
                                    }
                                }
                            }

                        }
                    }
            #endregion

            #region ParticleVis
                    ParticleSystem SpectrumParticle = audioSpectrumObjects[i].GetComponent<ParticleSystem>();
                    if (SpectrumParticle != null)
                    {
                        float intensity = spectrum[i] * heightMultiplier;

                        if (burstToggle)
                        {
                            var emission = SpectrumParticle.emission;
                            emission.rateOverDistance = 0f;
                            emission.rateOverTime = 0f;
                            ParticleSystem.Burst burst = new ParticleSystem.Burst();
                            burst.cycleCount = 0;
                            burst.time = burstTime;
                            float norm = Mathf.Lerp(emission.GetBurst(0).count.constant, intensity, lerpTime)* 100 * burstIntensity;
                            float count = (float)Math.Round(norm);
                            if(count >= minBurst && count <= maxBurst)
                            {
                                ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
                                minMax.constant = count;
                                burst.count = minMax;
                            }
                            else
                            {
                                ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
                                minMax.constant = 0f;
                                burst.count = minMax;
                            }
                            emission.SetBurst(0, burst);
                        }
                    }
            #endregion

            #region TransformVis
                    Transform SpectrumTransform = audioSpectrumObjects[i].GetComponent<Transform>();
                    if(SpectrumTransform != null)
                    {
                        float intensity = spectrum[i] * heightMultiplier;
                        if (transformToggle)
                        {
                            if (posToggle)
                            {
                                float lerpX_pos;
                                float lerpY_pos;
                                float lerpZ_pos;
                                if (posX)
                                {
                                    lerpX_pos = Mathf.Lerp(SpectrumTransform.localPosition.x, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpX_pos = SpectrumTransform.localPosition.x;
                                }
                                if (posY)
                                {
                                    lerpY_pos = Mathf.Lerp(SpectrumTransform.localPosition.y, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpY_pos = SpectrumTransform.localPosition.y;
                                }
                                if (posZ)
                                {
                                    lerpZ_pos = Mathf.Lerp(SpectrumTransform.localPosition.z, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpZ_pos = SpectrumTransform.localPosition.z;
                                }
                                Vector3 newPose = new Vector3(lerpX_pos, lerpY_pos, lerpZ_pos);

                                SpectrumTransform.localPosition = newPose;
                            }
                            if (rotateToggle)
                            {
                                float lerpX_rot;
                                float lerpY_rot;
                                float lerpZ_rot;
                                float lerpW_rot;
                                if (rotateX)
                                {
                                    lerpX_rot = Mathf.Lerp(SpectrumTransform.localRotation.x, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpX_rot = SpectrumTransform.localRotation.x;
                                }
                                if (rotateY)
                                {
                                    lerpY_rot = Mathf.Lerp(SpectrumTransform.localRotation.y, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpY_rot = SpectrumTransform.localRotation.y;
                                }
                                if (rotateZ)
                                {
                                    lerpZ_rot = Mathf.Lerp(SpectrumTransform.localRotation.z, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpZ_rot = SpectrumTransform.localRotation.z;
                                }
                                if (rotateW)
                                {
                                    lerpW_rot = Mathf.Lerp(SpectrumTransform.localRotation.w, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpW_rot = SpectrumTransform.localRotation.w;
                                }
                                Quaternion newRotate = new Quaternion(lerpX_rot, lerpY_rot, lerpZ_rot, lerpW_rot);

                                SpectrumTransform.localRotation = newRotate;
                            }
                            if (scaleToggle)
                            {
                                float lerpX_scale;
                                float lerpY_scale;
                                float lerpZ_scale;
                                if (scaleX)
                                {
                                    lerpX_scale = Mathf.Lerp(SpectrumTransform.localScale.x, intensity, lerpTime) + 0.2f;
                                }
                                else
                                {
                                    lerpX_scale = SpectrumTransform.localScale.x;
                                }
                                if (scaleY)
                                {
                                    lerpY_scale = Mathf.Lerp(SpectrumTransform.localScale.y, intensity, lerpTime) + 0.2f;
                                }
                                else
                                {
                                    lerpY_scale = SpectrumTransform.localScale.y;
                                }
                                if (scaleZ)
                                {
                                    lerpZ_scale = Mathf.Lerp(SpectrumTransform.localScale.z, intensity, lerpTime) + 0.2f;
                                }
                                else
                                {
                                    lerpZ_scale = SpectrumTransform.localScale.z;
                                }
                                Vector3 newScale = new Vector3(lerpZ_scale + scaleAddX, lerpY_scale + scaleAddY, lerpZ_scale + scaleAddZ);

                                SpectrumTransform.localScale = newScale;
                            }
                        }
                    }

            #endregion

            #region OrbitVis
                    Transform SpectrumOrbit = audioSpectrumObjects[i].GetComponent<Transform>();
                    if (SpectrumTransform != null)
                    {
                        if (orbitToggle && orbitCenterObject != null)
                        {
                            Vector3 orbitVec = new Vector3(orbitX, orbitY, orbitZ);
                            if (orbitVisualizer)
                            {
                                float intensity = spectrum[i] * heightMultiplier;
                                float speed = Mathf.Lerp(orbitSpeed, intensity, lerpTime);
                                int multi = 10 * orbitVisualizerSpeedMultiplyer;
                                SpectrumOrbit.RotateAround(orbitCenterObject.transform.position, orbitVec, (speed * multi) * Time.deltaTime);
                            }
                            else
                            {
                                SpectrumOrbit.RotateAround(orbitCenterObject.transform.position, orbitVec, orbitSpeed * Time.deltaTime);
                            }
                                
                        }
                    }
                    

            #endregion
                }

            #endregion
            }


        }
    }
#elif UNITY_EDITOR
            using UnityEditor;
    using UnityEditor.Animations;
    public class AudioVis : MonoBehaviour
        {
        [Header("Visualizer Settings")]
        public AudioSource audioSource;
        public GameObject[] audioSpectrumObjects;
        [Range(1, 100)] public float heightMultiplier = 1;
        [Range(64, 8192)] public int numberOfSamples = 1024; //step by 2
        public FFTWindow fftWindow;
        [Min(0f)] public float lerpTime = 1;

        [Header("Light Visualizer")]
        [Header("Light Intensity")]
        public bool intensityToggle;
        public int lightIntensity;
        [Header("Light Hue")]
        public bool HueToggle;
        [SerializeField] [Range(0f, 1f)] public float colorLerpTime = 0;
        private Color[] startColor;
        public Color[] lerpColor;
        [Range(0, 0.05f)] public float shiftSpeed = 0f;
        [SerializeField] [Range(0f, 1f)] public float colorShiftTime = 0;
        private bool isShifting = false;

        [Header("Particle Visualizer")]
        public bool burstToggle;
        public int burstIntensity;
        [Range(0, 10)] public float burstTime = 0f;
        [Min(0)] public int minBurst = 0;
        [Min(1)] public int maxBurst = 5;

        [Header("Transform Visualizer")]
        public bool transformToggle;

        [Header("Transform Position Settings")]
        public bool posToggle;
        public bool posX;
        public bool posY;
        public bool posZ;

        [Header("Transform Rotation Settings")]
        public bool rotateToggle;
        public bool rotateX;
        public bool rotateY;
        public bool rotateZ;
        public bool rotateW;

        [Header("Transform Scale Settings")]
        public bool scaleToggle;
        public bool scaleX;
        public bool scaleY;
        public bool scaleZ;

        [Header("Orbit Settings")]
        public bool orbitToggle;
        public GameObject orbitCenterObject;
        public bool orbitVisualizer;
        [Range(1, 100)] public int orbitVisualizerSpeedMultiplyer = 10;
        public float orbitSpeed;

        public float orbitX;
        public float orbitY;
        public float orbitZ;

        [Header("Recorder Settings")]
        public bool record;
        public AnimationClip clip;
        private GameObjectRecorder m_Recorder;
        public bool recordOnlyWhilePlayMusic;

        private Color HueShift(Color color, float hueShiftAmount, float lTime)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            h += hueShiftAmount;
            Color output = Color.HSVToRGB(h, s, v, true);
            Color Lerp = Color.Lerp(color, output, lTime);
            float h1, s1, v1;
            Color.RGBToHSV(Lerp, out h1, out s1, out v1);
            return Color.HSVToRGB(h1, s, v,true);
        }


        void Start()
        {
            startColor = new Color[audioSpectrumObjects.Length];
            for (int i = 0; i < audioSpectrumObjects.Length; i++)
            {
                Light SpectrumLight = audioSpectrumObjects[i].GetComponent<Light>();
                if (SpectrumLight != null)
                    startColor[i] = SpectrumLight.color;
            }

            if (record)
            {
                // Create recorder and record the script GameObject.
                m_Recorder = new GameObjectRecorder(gameObject);

                // Bind all the Transforms on the GameObject and all its children.
                m_Recorder.BindComponentsOfType<Transform>(gameObject, true);
            }
            if (HueToggle && shiftSpeed == 0f && lerpColor.Length != audioSpectrumObjects.Length)
            {
                lerpColor = new Color[audioSpectrumObjects.Length];
            }
        }

        void LateUpdate()
        {
            if (record)
            {
                if (clip == null)
                    return;

                // Take a snapshot and record all the bindings values for this frame.
                if (recordOnlyWhilePlayMusic)
                {
                    if (audioSource.isPlaying)
                        m_Recorder.TakeSnapshot(Time.deltaTime);
                }
                else
                {
                    m_Recorder.TakeSnapshot(Time.deltaTime);
                }
                
            }

        }

        void OnDisable()
        {
            if (record)
            {
                if (clip == null)
                    return;

                if (m_Recorder.isRecording)
                {
                    // Save the recorded session to the clip.
                    m_Recorder.SaveToClip(clip);
                }
            }

        }

        void Update()
        {
            if (audioSource != null)
            {
                #region audioVis
                float[] spectrum = new float[numberOfSamples];

                audioSource.GetSpectrumData(spectrum, 0, fftWindow);

                


                for (int i = 0; i < audioSpectrumObjects.Length; i++)
                {
                    #region LightVis
                    Light SpectrumLight = audioSpectrumObjects[i].GetComponent<Light>();
                    if (SpectrumLight != null)
                    {
                        float intensity = spectrum[i] * heightMultiplier;

                        if (intensityToggle)
                        {
                            SpectrumLight.intensity = Mathf.Lerp(SpectrumLight.intensity, intensity, lerpTime) * lightIntensity;
                        }

                        if (HueToggle)
                        {
                            if (SpectrumLight.color.r == 0 && SpectrumLight.color.g == 0 && SpectrumLight.color.b == 0)
                            {
                                SpectrumLight.color = new Vector4(0.5f, 0.75f, 0.25f, 1f);
                            }
                            else
                            {

                                if(shiftSpeed == 0f)
                                {
                                    if (SpectrumLight.color == startColor[i])
                                    {
                                        isShifting = true;
                                    }
                                    else if (SpectrumLight.color == lerpColor[i])
                                    {
                                        isShifting = false;
                                    }

                                    if (isShifting)
                                    {
                                        SpectrumLight.color = Color.Lerp(SpectrumLight.color, lerpColor[i], colorLerpTime);
                                    }
                                    else
                                    {
                                        SpectrumLight.color = Color.Lerp(SpectrumLight.color, startColor[i], colorLerpTime);
                                    }
                                }
                                else
                                {
                                    Color gray = new Vector4(0.494f, 0.494f, 0.494f, 1f);
                                    if (SpectrumLight.color.Equals(gray))
                                    {
                                        SpectrumLight.color = Color.red;
                                    }
                                    else
                                    {
                                        SpectrumLight.color = HueShift(SpectrumLight.color, shiftSpeed, colorShiftTime);
                                    }
                                }

                            }

                        }
                    }
                    #endregion

                    #region ParticleVis
                    ParticleSystem SpectrumParticle = audioSpectrumObjects[i].GetComponent<ParticleSystem>();
                    if (SpectrumParticle != null)
                    {
                        float intensity = spectrum[i] * heightMultiplier;

                        if (burstToggle)
                        {
                            var emission = SpectrumParticle.emission;
                            emission.rateOverDistance = 0f;
                            emission.rateOverTime = 0f;
                            ParticleSystem.Burst burst = new ParticleSystem.Burst();
                            burst.cycleCount = 0;
                            burst.time = burstTime;
                            float norm = Mathf.Lerp(emission.GetBurst(0).count.constant, intensity, lerpTime) * 100 * burstIntensity;
                            float count = (float)Math.Round(norm);
                            if (count >= minBurst && count <= maxBurst)
                            {
                                ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
                                minMax.constant = count;
                                burst.count = minMax;
                            }
                            else
                            {
                                ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
                                minMax.constant = 0f;
                                burst.count = minMax;
                            }
                            emission.SetBurst(0, burst);
                        }
                    }
                    #endregion

                    #region TransformVis
                    Transform SpectrumTransform = audioSpectrumObjects[i].GetComponent<Transform>();
                    if (SpectrumTransform != null)
                    {
                        float intensity = spectrum[i] * heightMultiplier;
                        if (transformToggle)
                        {
                            if (posToggle)
                            {
                                float lerpX_pos;
                                float lerpY_pos;
                                float lerpZ_pos;
                                if (posX)
                                {
                                    lerpX_pos = Mathf.Lerp(SpectrumTransform.localPosition.x, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpX_pos = SpectrumTransform.localPosition.x;
                                }
                                if (posY)
                                {
                                    lerpY_pos = Mathf.Lerp(SpectrumTransform.localPosition.y, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpY_pos = SpectrumTransform.localPosition.y;
                                }
                                if (posZ)
                                {
                                    lerpZ_pos = Mathf.Lerp(SpectrumTransform.localPosition.z, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpZ_pos = SpectrumTransform.localPosition.z;
                                }
                                Vector3 newPose = new Vector3(lerpX_pos, lerpY_pos, lerpZ_pos);

                                SpectrumTransform.localPosition = newPose;
                            }
                            if (rotateToggle)
                            {
                                float lerpX_rot;
                                float lerpY_rot;
                                float lerpZ_rot;
                                float lerpW_rot;
                                if (rotateX)
                                {
                                    lerpX_rot = Mathf.Lerp(SpectrumTransform.localRotation.x, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpX_rot = SpectrumTransform.localRotation.x;
                                }
                                if (rotateY)
                                {
                                    lerpY_rot = Mathf.Lerp(SpectrumTransform.localRotation.y, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpY_rot = SpectrumTransform.localRotation.y;
                                }
                                if (rotateZ)
                                {
                                    lerpZ_rot = Mathf.Lerp(SpectrumTransform.localRotation.z, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpZ_rot = SpectrumTransform.localRotation.z;
                                }
                                if (rotateW)
                                {
                                    lerpW_rot = Mathf.Lerp(SpectrumTransform.localRotation.w, intensity, lerpTime);
                                }
                                else
                                {
                                    lerpW_rot = SpectrumTransform.localRotation.w;
                                }
                                Quaternion newRotate = new Quaternion(lerpX_rot, lerpY_rot, lerpZ_rot, lerpW_rot);

                                SpectrumTransform.localRotation = newRotate;
                            }
                            if (scaleToggle)
                            {
                                float lerpX_scale;
                                float lerpY_scale;
                                float lerpZ_scale;
                                if (scaleX)
                                {
                                    lerpX_scale = Mathf.Lerp(SpectrumTransform.localScale.x, intensity, lerpTime) + 0.2f;
                                }
                                else
                                {
                                    lerpX_scale = SpectrumTransform.localScale.x;
                                }
                                if (scaleY)
                                {
                                    lerpY_scale = Mathf.Lerp(SpectrumTransform.localScale.y, intensity, lerpTime) + 0.2f;
                                }
                                else
                                {
                                    lerpY_scale = SpectrumTransform.localScale.y;
                                }
                                if (scaleZ)
                                {
                                    lerpZ_scale = Mathf.Lerp(SpectrumTransform.localScale.z, intensity, lerpTime) + 0.2f;
                                }
                                else
                                {
                                    lerpZ_scale = SpectrumTransform.localScale.z;
                                }
                                Vector3 newScale = new Vector3(lerpZ_scale, lerpY_scale, lerpZ_scale);

                                SpectrumTransform.localScale = newScale;
                            }
                        }
                    }

                    #endregion

                    #region OrbitVis
                    Transform SpectrumOrbit = audioSpectrumObjects[i].GetComponent<Transform>();
                    if (SpectrumTransform != null)
                    {
                        if (orbitToggle && orbitCenterObject != null)
                        {
                            Vector3 orbitVec = new Vector3(orbitX, orbitY, orbitZ);
                            if (orbitVisualizer)
                            {
                                float intensity = spectrum[i] * heightMultiplier;
                                float speed = Mathf.Lerp(orbitSpeed, intensity, lerpTime);
                                int multi = 10 * orbitVisualizerSpeedMultiplyer;
                                SpectrumOrbit.RotateAround(orbitCenterObject.transform.position, orbitVec, (speed * multi) * Time.deltaTime);
                            }
                            else
                            {
                                SpectrumOrbit.RotateAround(orbitCenterObject.transform.position, orbitVec, orbitSpeed * Time.deltaTime);
                            }

                        }
                    }


                    #endregion


                }

                #endregion
            }


        }
    }
#endif
}




