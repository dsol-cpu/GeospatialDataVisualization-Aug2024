using UnityEngine.Jobs;
using Unity.Burst;
using TMPro;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;

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

        private JobHandle jobHandle;
        private static InstancePinMeshManager _instance;

        public static InstancePinMeshManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InstancePinMeshManager>();
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
            Transform[] pinTransforms = new Transform[count];

            for (int i = 0; i < count; ++i)
            {
                Vector3 position = V360_Utilities.LLAToXYZ((float)locationsl[i].Latitude, (float)locationsl[i].Longitude);
                position = position.normalized * (position.magnitude + pinObject.transform.position.y);
                GameObject pin = Instantiate(pinObject, position, Quaternion.identity);
                pinTransforms[i] = pin.transform;
                pin.transform.SetParent(referenceGlobeTransform);
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

        void Update()
        {
            if (previousMousePosition != Input.mousePosition)
            {
                previousMousePosition = Input.mousePosition;

                UpdateTransformsJob job = new UpdateTransformsJob
                {
                    referencePosition = referenceGlobeTransform.position,
                    referenceRotation = referenceGlobeTransform.rotation,
                    localPositions = pinLocalPositions,
                    positions = pinPositions,
                    rotations = pinRotations
                };
                jobHandle = job.Schedule(pinTransformsArray);

                if (Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(previousMousePosition), out RaycastHit hit))
                {
                    Transform hitTransform = hit.transform;
                    int hoveredIndex = -1;

                    for (int i = 0; i < pinTransformsArray.length; i++)
                    {
                        if (pinTransformsArray[i].Equals(hitTransform))
                        {
                            hoveredIndex = i;
                            break;
                        }
                    }

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
                else if (lastActiveIndex != -1)
                {
                    uiTextDisplay.text = "Hover over a Location";
                    lastActiveIndex = -1;
                }
            }
        }

        void LateUpdate()
        {
            jobHandle.Complete();
        }

        private void OnDestroy()
        {
            if (pinLocalPositions.IsCreated) pinLocalPositions.Dispose();
            if (pinPositions.IsCreated) pinPositions.Dispose();
            if (pinRotations.IsCreated) pinRotations.Dispose();
            if (pinTransformsArray.isCreated) pinTransformsArray.Dispose();
        }

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
                Vector3 worldPosition = referenceRotation * localPositions[index] + referencePosition;
                Vector3 directionToCenter = referencePosition - worldPosition;
                Quaternion lookAtRotation = Quaternion.LookRotation(directionToCenter, Vector3.up);

                transform.rotation = lookAtRotation;
            }
        }
    }
}