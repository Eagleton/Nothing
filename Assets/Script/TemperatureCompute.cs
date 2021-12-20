using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureCompute : MonoBehaviour
{
    private const float Deg2Rad = Mathf.PI / 180.0f;


    private int DaysOfYear(int year)
    {
        if (year % 400 == 0 || (year % 100 != 0 && year % 4 == 0))
        {
            return 366;
        }
        else return 365;
    }

    private float CalculateSunTotalRadiation(int year, int daysInYear, float hoursInday, float latitude, float atmosphereTransparency)
    {
        float solarDeclination;     //赤纬角
        float sunTimeAngle;         //太阳时角
        float sunHeightAngle;       //太阳高度角
        float incidentAngle;        //太阳入射角
        float planeAngle = 0f;      //壁面角度
        float limita;               //太阳直射误差项
        float ipsl;                 //壁面太阳方向
        float atmosphereQuality;    //大气质量

        int daysOfYear = DaysOfYear(year);
        solarDeclination = Mathf.Asin(0.039795f * Mathf.Cos(0.098563f * (daysInYear - 173)));
        sunTimeAngle = (hoursInday - 12.0f) * 15.0f * Deg2Rad; 
        sunHeightAngle = Mathf.Asin(Mathf.Sin(latitude * Deg2Rad) * Mathf.Sin(solarDeclination) + Mathf.Cos(solarDeclination) * Mathf.Cos(latitude * Deg2Rad) * Mathf.Cos(sunTimeAngle));
        ipsl = Mathf.Asin(Mathf.Cos(solarDeclination) * Mathf.Sin(sunTimeAngle) / Mathf.Cos(sunHeightAngle));
        incidentAngle = Mathf.Asin(Mathf.Cos(planeAngle) * Mathf.Sin(sunHeightAngle) + Mathf.Sin(planeAngle) * Mathf.Cos(sunHeightAngle) * Mathf.Cos(ipsl));
        if(sunHeightAngle > 0)
        {
            limita = 1 + 0.034f * Mathf.Cos(daysInYear * 2 * Mathf.PI / daysOfYear);

            
        }


        return 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
