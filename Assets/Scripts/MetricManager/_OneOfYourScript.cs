using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _OneOfYourScript : MonoBehaviour
{
    /// <summary>
    /// If you want to collect the data periodically. Please use this chunk of code - Start - 
    /// </summary>
    void Start()
    {
        MetricsEvents.OnDataCollect += this.CollectData;
    }
    public void CollectData()
    {
        if (MetricManagerScript.instance != null)
        {
            MetricManagerScript.instance.LogString("Time-based Variable name", "Data Point");
        }
    }

    /// <summary>
    /// If you want to collect the data periodically. Please use this chunk of code - End - 
    /// </summary>



    public void ButtonPressed()
    {
        /// <summary>
        /// If you want to collect the data when the event happens. Please use this chunk of code - Start - 
        /// </summary>
        if (MetricManagerScript.instance != null)
        {
            MetricManagerScript.instance.LogString("Event-based Variable name", "Data Point");
        }
        /// <summary>
        /// If you want to collect the data when the event happens. Please use this chunk of code - End - 
        /// </summary>
    }
}
