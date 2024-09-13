using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts
{
    public class AESCodeChallenge : MonoBehaviour
    {
        [SerializeField] private string dataFilePath;
        [SerializeField] private GameObject pinGameObject;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            if (string.IsNullOrEmpty(dataFilePath))
            {
                Debug.LogError("Data file path is not assigned!");
                return;
            }

            if (pinGameObject == null)
            {
                Debug.LogError("Mesh Prefab is not assigned!");
                return;
            }
            List<Location> locationsList = ParsedData.LoadCityData(dataFilePath);
            Debug.Log($"Object: {pinGameObject}, Locations: {locationsList.Count}");
            InstancePinMeshManager.Instance.InstantiatePins(locationsList.Count, pinGameObject, locationsList);
        }
    }
}