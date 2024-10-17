using UnityEngine;

namespace Venwin.CommonScripts
{
    public class RotateObject : MonoBehaviour
    {
        [SerializeField] float rotateSpeed;
        [SerializeField] Vector3 rotationDirection;


        // Update is called once per frame
        void Update()
        {
            transform.Rotate(rotationDirection * Time.deltaTime * rotateSpeed);
        }
    }
}
