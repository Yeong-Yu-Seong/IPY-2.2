/*
    Author: Yeong Yu Seong
    Date: 19 January 2026
    Description: This script makes a GameObject spin around its Y-axis.
*/

using UnityEngine;

public class Spin : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the object around its Y-axis at 360 degrees per second
        transform.Rotate(0, 360 * Time.deltaTime, 0);
    }
}
