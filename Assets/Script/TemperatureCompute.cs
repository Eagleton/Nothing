using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureCompute : MonoBehaviour
{
    [Header("波长")]
    public float wavelength1 = 3f;  //波段下限
    public float wavelength2 = 5f;  //波段上限

    [Header("环境参数")]
    public float atmosphereTransparency;    //大气透明度
    public float maxTemperature;            //最高温度
    public float minTemperature;            //最低温度
    public float maxHumidity;               //最高湿度
    public float minHumidity;               //最低湿度
    public float windSpeed;                 //风速
    public float ccf;                       //云遮系数

    [Header("时间和时区")]
    public int year;            //年 
    public int month;           //月      
    public int day;             //天
    public int Hour;            //小时    
    public int Min;             //分钟
    public float UTC;           //时区
    public int daysInYear;      //一年中的天数
    public float hoursInDay;    //一天中的小时数

    [Header("地理位置")]
    public float latitude;      //经度
    public float longitude;     //纬度

    private const float Deg2Rad = Mathf.PI / 180.0f;

    private float solarRadiation;
    private float skyRadiation;

    private readonly int MAX_ITERATION_PER_STEP = 100;
    private readonly float EPSION = 1e-6f;


    public struct pMaterial
    {
        public int no; // 编号
        public float longWaveEmissivity;              // 长波发射率
        public float midWaveEmissivity;               // 中波发射率
        public float sunWaveEmissivity;               // 太阳波段发射率
        public float density;                 // 密度 kg/m^3
        public float heatCapacity;            // 比热容 J/(kg*℃)
        public float heatTransferCoefficient; // 导热系数 W/(m*K)
    };

    private void CalculateDayInYearAndHourInDay()
    {
        //计算某日在一年中的天数
        if (month == 1)
            daysInYear = day;
        else if (month == 2)
            daysInYear = 31 + day;
        else if (month == 3)
            daysInYear = 31 + 28 + day;
        else if (month == 4)
            daysInYear = 31 + 28 + 31 + day;
        else if (month == 5)
            daysInYear = 31 + 28 + 31 + 30 + day;
        else if (month == 6)
            daysInYear = 31 + 28 + 31 + 30 + 31 + day;
        else if (month == 7)
            daysInYear = 31 + 28 + 31 + 30 + 31 + 30 + day;
        else if (month == 8)
            daysInYear = 31 + 28 + 31 + 30 + 31 + 30 + 31 + day;
        else if (month == 9)
            daysInYear = 31 + 28 + 31 + 30 + 31 + 30 + 31 + 31 + day;
        else if (month == 10)
            daysInYear = 31 + 28 + 31 + 30 + 31 + 30 + 31 + 31 + 30 + day;
        else if (month == 11)
            daysInYear = 31 + 28 + 31 + 30 + 31 + 30 + 31 + 31 + 30 + 31 + day;
        else if (month == 12)
            daysInYear = 31 + 28 + 31 + 30 + 31 + 30 + 31 + 31 + 30 + 31 + 30 + day;
        else;
        if ((year % 100 != 0 && year % 4 == 0) || year % 400 == 0) //闰年二月多一天
            daysInYear += 1;

    }

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
        float sunHeightAngle;       //太阳高度角h
        float incidentAngle;        //太阳入射角
        float planeAngle = 0f;      //壁面角度
        float limita;               //太阳直射误差项
        float ipsl;                 //壁面太阳方向
        float atmosphereQuality;    //大气质量
        float sunDirectRadiation;   //太阳直射辐射
        float skyScatterRadiation;  //天空散射辐射

        int daysOfYear = DaysOfYear(year);
        solarDeclination = Mathf.Asin(0.039795f * Mathf.Cos(0.098563f * (daysInYear - 173)));
        sunTimeAngle = (hoursInday - 12.0f) * 15.0f * Deg2Rad; 
        sunHeightAngle = Mathf.Asin(Mathf.Sin(latitude * Deg2Rad) * Mathf.Sin(solarDeclination) + Mathf.Cos(solarDeclination) * Mathf.Cos(latitude * Deg2Rad) * Mathf.Cos(sunTimeAngle));
        ipsl = Mathf.Asin(Mathf.Cos(solarDeclination) * Mathf.Sin(sunTimeAngle) / Mathf.Cos(sunHeightAngle));
        incidentAngle = Mathf.Asin(Mathf.Cos(planeAngle) * Mathf.Sin(sunHeightAngle) + Mathf.Sin(planeAngle) * Mathf.Cos(sunHeightAngle) * Mathf.Cos(ipsl)); //太阳入射角
        if(sunHeightAngle > 0)
        {
            limita = 1 + 0.034f * Mathf.Cos(daysInYear * 2 * Mathf.PI / daysOfYear); //误差项
            atmosphereQuality = 1.0f / Mathf.Sin(sunHeightAngle);   //m = 1/sinh
            float p = Mathf.Pow(atmosphereTransparency, atmosphereQuality);
            sunDirectRadiation = 1353.0f * p * limita * Mathf.Cos(incidentAngle);
            skyScatterRadiation = 0.5f - 1353.0f * Mathf.Sin(sunHeightAngle) * (1 - p) / (1.0f - 1.4f * -Mathf.Log(atmosphereTransparency)) * Mathf.Cos(planeAngle * Mathf.PI / 360.0f);
            return sunDirectRadiation + skyScatterRadiation;
        }
        else 
            return 0.0f;
    }

    private float MultiLayerFiniteDifference(int dxNum, float environmentTemp, pMaterial m)
    {
        float dt = 10;          //时间步长1s
        float dx = 0.01f;       //空间步长0.01m
        int n = dxNum;          //空间单元数量
        float initTemprature = environmentTemp;
        float boundHeat = 15.0f;
        float QSun = 0f;
        float QSky = 0f;
        float QEmit = 0f;
        float Qcd = 0f;
        float Qcv = 0f;

        bool isInsulation = true;
        const int maxArrayCount = 103;
        float[] lastTemps = new float[maxArrayCount];
        float[] Temps = new float[maxArrayCount];


        float ratio = m.heatTransferCoefficient / (m.density * m.heatCapacity);
        float fo = ratio * dt / (dx * dx); // 傅里叶数

        float diff = boundHeat - initTemprature;
        for(int i  = 0; i < n; i++)
        {
            Temps[i] = initTemprature + i * diff / n;
            lastTemps[i] = 0f;
        }

        for(int k = 0; k < n; k++)
        {
            if(k == 0) //表面
            {
                Temps[k] = lastTemps[k] + dt * (QSun + QSky - Qcv - Qcd - QEmit) / (m.density * m.heatCapacity * dx);
            }
            else if(k == n - 1) //最底层
            {
                float lastLayerTemp = lastTemps[k - 1];
                float nextLayerTemp = boundHeat;
                if (isInsulation)
                    nextLayerTemp = lastTemps[k];
                Temps[k] = lastTemps[k] + ratio * dt * (nextLayerTemp - lastTemps[k] * 2 + lastLayerTemp) / (dx * dx); //中心差分

            }
            else //中间层，差分
            {
                float lastLayerTemp = lastTemps[k - 1];
                float nextLayerTemp = lastTemps[k + 1]; 
                Temps[k] = lastTemps[k] + ratio * dt * (nextLayerTemp - lastTemps[k] * 2 + lastLayerTemp) / (dx * dx); //中心差分
            }
        }
        
        return Temps[0]; //只要表层温度
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
