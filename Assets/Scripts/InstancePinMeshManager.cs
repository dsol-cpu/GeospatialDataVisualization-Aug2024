using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Burst;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts
{
    using Quaternion = UnityEngine.Quaternion;
    using Vector3 = UnityEngine.Vector3;
    public class InstancePinMeshManager : MonoBehaviour
    {
        [SerializeField] Transform referenceGlobeTransform;
        [SerializeField] Transform referenceCameraTransform;
        [SerializeField] TMP_Text uiTextDisplay;
        private Location[] locationList;
        private TransformAccessArray pinTransformsArray;
        private NativeArray<Vector3> pinLocalPositions;
        private NativeArray<Vector3> pinPositions;
        private NativeArray<Quaternion> pinRotations;
        private Quaternion previousRotation;

        private Vector3 previousMousePosition;
        [SerializeField] private int lastActiveIndex = -1;

        private bool matchFound;
        private JobHandle jobHandle;
        private static InstancePinMeshManager _instance;

        public static InstancePinMeshManager Instance
        {
            get
            {
                // If instance is null, find it in the scene
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InstancePinMeshManager>();

                    // If not found, create a new instance
                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject("InstancePinMeshManager");
                        _instance = singleton.AddComponent<InstancePinMeshManager>();
                    }
                }
                return _instance;
            }
        }

        void Start()
        {
            previousRotation = referenceGlobeTransform.rotation;
        }

        public void InstantiatePins(int count, GameObject pinObject, List<Location> locationsl)
        {
            if (pinObject == null)
            {
                Debug.LogError("Invalid input!");
                return;
            }
            locationList = locationsl.ToArray();

            if (!pinObject.activeSelf)
                pinObject.SetActive(true);

            Transform[] pinTransforms = new Transform[count];

            //instantiate the pins
            for (int i = 0; i < count; ++i)
            {
                UnityEngine.Vector3 position = V360_Utilities.LLAToXYZ((float)locationsl[i].Latitude, (float)locationsl[i].Longitude);
                position = position.normalized * (position.magnitude + pinObject.transform.position.y); // Apply offset

                GameObject pin = Instantiate(pinObject, position, UnityEngine.Quaternion.identity);
                pinTransforms[i] = pin.transform;
            }
            pinLocalPositions = new NativeArray<Vector3>(count, Allocator.Persistent);
            pinPositions = new NativeArray<Vector3>(count, Allocator.Persistent);
            pinRotations = new NativeArray<Quaternion>(count, Allocator.Persistent);

            for (int i = 0; i < count; i++)
            {
                pinLocalPositions[i] = pinTransforms[i].position - referenceGlobeTransform.position;
            }

            pinTransformsArray = new TransformAccessArray(pinTransforms);

            pinObject.SetActive(false);
        }

        private static void ScalePin(Transform pin, float scaleFactor)
        {
            pin.localScale *= scaleFactor;
        }

        void Update()
        {
            // Check for mouse position change
            if (previousMousePosition != Input.mousePosition)
            {
                previousMousePosition = Input.mousePosition;
                // Always schedule the job to update transforms
                UpdateTransformsJob job = new UpdateTransformsJob
                {
                    referencePosition = referenceGlobeTransform.position,
                    referenceRotation = referenceGlobeTransform.rotation,
                    localPositions = pinLocalPositions,
                    positions = pinPositions,
                    rotations = pinRotations
                };
                jobHandle = job.Schedule(pinTransformsArray);

                // Check for raycast hit
                if (Physics.Raycast(Camera.main.ScreenPointToRay(previousMousePosition), out RaycastHit hit))
                {
                    Transform hitTransform = hit.transform;
                    Debug.Log($"Trying at index: {lastActiveIndex}");

                    // Find the index of the hitTransform in the pinTransformsArray
                    int hoveredIndex = -1;

                    if (hitTransform.position != referenceGlobeTransform.position)
                    {
                        for (int i = 0; i < pinTransformsArray.length; i++)
                        {
                            if (pinTransformsArray[i].Equals(hitTransform))
                            {
                                Debug.Log("Found!");
                                hoveredIndex = i;
                                break;
                            }
                        }
                    }

                    // Update UI based on the hoveredIndex
                    if (hoveredIndex != -1)
                    {
                        uiTextDisplay.text = locationList[hoveredIndex].ToString();
                        lastActiveIndex = hoveredIndex;
                    }
                    else
                    {
                        uiTextDisplay.text = "Hover over a Location";
                        lastActiveIndex = -1;
                    }
                }
                else
                {
                    // Reset UI if no hit
                    if (lastActiveIndex != -1)
                    {
                        uiTextDisplay.text = "Hover over a Location";
                        lastActiveIndex = -1;
                    }
                }
            }
            // Ensure job is completed if needed before further processing
            // This depends on when and how you need the job results
        }


        /// <summary>
        /// LateUpdate is called every frame, if the Behaviour is enabled.
        /// It is called after all Update functions have been called.
        /// </summary>
        void LateUpdate()
        {
            jobHandle.Complete();
        }

        private void OnDestroy()
        {
            if (pinLocalPositions.IsCreated) pinLocalPositions.Dispose();
            if (pinPositions.IsCreated) pinPositions.Dispose();
            if (pinRotations.IsCreated) pinRotations.Dispose();
        }


        // Job to update the pin rotations
        [BurstCompile]
        private struct UpdateTransformsJob : IJobParallelForTransform
        {
            public Vector3 referencePosition;
            public Quaternion referenceRotation;

            [ReadOnly] public NativeArray<Vector3> localPositions;
            [WriteOnly] public NativeArray<Vector3> positions;
            [WriteOnly] public NativeArray<Quaternion> rotations;

            public void Execute(int index, TransformAccess transform)
            {
                // Calculate the new world position of the pin
                Vector3 worldPosition = referenceRotation * localPositions[index] + referencePosition;

                Vector3 directionToCenter = referencePosition - worldPosition;
                // Calculate the rotation so the pin points towards the center of the globe
                Quaternion lookAtRotation = Quaternion.LookRotation(directionToCenter, Vector3.up);

                // Store the updated rotation
                transform.position = worldPosition;
                transform.rotation = lookAtRotation;
            }
        }
    }
}