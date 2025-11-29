using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Collections;

public class WeatherTimeSystem : MonoBehaviour
{
    public enum TimeOfDay { Day, Dusk, Night }
    public enum WeatherType { Clear, Cloudy, Rain, Snow }

    [Header("=== 直接光照配置（每个组合独立调整）===")]
    // 白天（Day）光照配置
    [Tooltip("白天晴天 - 光照强度")] public float dayClearLightIntensity = 0.5f;
    [Tooltip("白天晴天 - 光照颜色")] public Color dayClearLightColor = new Color(1f, 0.95f, 0.9f);
    [Tooltip("白天阴天 - 光照强度")] public float dayCloudyLightIntensity = 0.4f;
    [Tooltip("白天阴天 - 光照颜色")] public Color dayCloudyLightColor = new Color(0.9f, 0.9f, 0.9f);
    [Tooltip("白天下雨 - 光照强度")] public float dayRainLightIntensity = 0.3f;
    [Tooltip("白天下雨 - 光照颜色")] public Color dayRainLightColor = new Color(0.8f, 0.85f, 0.9f);
    [Tooltip("白天下雪 - 光照强度")] public float daySnowLightIntensity = 0.35f;
    [Tooltip("白天下雪 - 光照颜色")] public Color daySnowLightColor = new Color(0.95f, 0.95f, 1f);

    // 黄昏（Dusk）光照配置
    [Tooltip("黄昏晴天 - 光照强度")] public float duskClearLightIntensity = 0.3f;
    [Tooltip("黄昏晴天 - 光照颜色")] public Color duskClearLightColor = new Color(0.9f, 0.6f, 0.4f);
    [Tooltip("黄昏阴天 - 光照强度")] public float duskCloudyLightIntensity = 0.25f;
    [Tooltip("黄昏阴天 - 光照颜色")] public Color duskCloudyLightColor = new Color(0.8f, 0.55f, 0.4f);
    [Tooltip("黄昏下雨 - 光照强度")] public float duskRainLightIntensity = 0.2f;
    [Tooltip("黄昏下雨 - 光照颜色")] public Color duskRainLightColor = new Color(0.7f, 0.5f, 0.4f);
    [Tooltip("黄昏下雪 - 光照强度")] public float duskSnowLightIntensity = 0.22f;
    [Tooltip("黄昏下雪 - 光照颜色")] public Color duskSnowLightColor = new Color(0.85f, 0.6f, 0.45f);

    // 夜晚（Night）光照配置
    [Tooltip("夜晚晴天 - 光照强度")] public float nightClearLightIntensity = 0.1f;
    [Tooltip("夜晚晴天 - 光照颜色")] public Color nightClearLightColor = new Color(0.3f, 0.4f, 0.8f);
    [Tooltip("夜晚阴天 - 光照强度")] public float nightCloudyLightIntensity = 0.08f;
    [Tooltip("夜晚阴天 - 光照颜色")] public Color nightCloudyLightColor = new Color(0.25f, 0.35f, 0.7f);
    [Tooltip("夜晚下雨 - 光照强度")] public float nightRainLightIntensity = 0.05f;
    [Tooltip("夜晚下雨 - 光照颜色")] public Color nightRainLightColor = new Color(0.2f, 0.3f, 0.6f);
    [Tooltip("夜晚下雪 - 光照强度")] public float nightSnowLightIntensity = 0.09f;
    [Tooltip("夜晚下雪 - 光照颜色")] public Color nightSnowLightColor = new Color(0.4f, 0.5f, 0.9f);

    [Header("Skybox Settings - Time + Weather Combinations")]
    public Material dayClearSkybox;
    public Material dayCloudySkybox;
    public Material dayRainSkybox;
    public Material daySnowSkybox;
    public Material duskClearSkybox;
    public Material duskCloudySkybox;
    public Material duskRainSkybox;
    public Material duskSnowSkybox;
    public Material nightClearSkybox;
    public Material nightCloudySkybox;
    public Material nightRainSkybox;
    public Material nightSnowSkybox;

    [Header("Time Buttons")]
    public Button dayBtn;
    public Button duskBtn;
    public Button nightBtn;

    [Header("Weather Buttons")]
    public Button clearBtn;
    public Button cloudyBtn;
    public Button rainBtn;
    public Button snowBtn;

    [Header("Scene Components")]
    public Light mainLight;
    public TextMeshProUGUI statusText;
    public ParticleSystem rainParticle;
    public ParticleSystem snowParticle;

    [Header("Real-time Weather Settings")]
    public RealTimeWeatherFetcher realTimeFetcher;
    public Toggle realTimeToggle;
    public Button refreshRealTimeBtn;

    // 当前状态
    private TimeOfDay currentTime = TimeOfDay.Day;
    private WeatherType currentWeather = WeatherType.Clear;

    void Start()
    {
        // 原有初始化逻辑（无修改）
        ApplyCurrentSettings();
        BindManualButtons();

        if (realTimeToggle != null)
        {
            realTimeToggle.onValueChanged.AddListener(OnRealTimeToggleChanged);
            UpdateRealTimeToggleText();
        }

        if (refreshRealTimeBtn != null)
        {
            refreshRealTimeBtn.onClick.AddListener(RefreshRealTimeData);
        }

        if (realTimeFetcher != null)
        {
            realTimeFetcher.OnRealDataUpdated += OnRealDataUpdated;
        }
    }

    // 原有核心逻辑（无修改）
    private void BindManualButtons()
    {
        dayBtn.onClick.AddListener(() => SetTime(TimeOfDay.Day));
        duskBtn.onClick.AddListener(() => SetTime(TimeOfDay.Dusk));
        nightBtn.onClick.AddListener(() => SetTime(TimeOfDay.Night));
        clearBtn.onClick.AddListener(() => SetWeather(WeatherType.Clear));
        cloudyBtn.onClick.AddListener(() => SetWeather(WeatherType.Cloudy));
        rainBtn.onClick.AddListener(() => SetWeather(WeatherType.Rain));
        snowBtn.onClick.AddListener(() => SetWeather(WeatherType.Snow));
    }

    private void SetTime(TimeOfDay time)
    {
        if (realTimeToggle != null && realTimeToggle.isOn)
        {
            realTimeToggle.isOn = false;
            UpdateRealTimeToggleText();
        }

        if (currentTime == time) return;

        currentTime = time;
        ApplyCurrentSettings();
    }

    private void SetWeather(WeatherType weather)
    {
        if (realTimeToggle != null && realTimeToggle.isOn)
        {
            realTimeToggle.isOn = false;
            UpdateRealTimeToggleText();
        }

        if (currentWeather == weather) return;

        currentWeather = weather;
        ApplyCurrentSettings();
    }

    // 关键修改：直接应用对应组合的光照参数
    private void ApplyCurrentSettings()
    {
        // 原有天空盒逻辑（无修改）
        Material skybox = GetSkybox(currentTime, currentWeather);
        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
            DynamicGI.UpdateEnvironment();
        }

        // 核心：根据当前时间+天气，直接读取对应的独立光照参数
        (float intensity, Color color) = GetCurrentLightParams();
        mainLight.intensity = intensity;
        mainLight.color = color;

        // 原有粒子控制逻辑（无修改）
        ControlParticles(currentWeather);
        UpdateStatusText();
    }

    // 新增：直接映射当前组合的光照参数
    private (float intensity, Color color) GetCurrentLightParams()
    {
        switch (currentTime)
        {
            case TimeOfDay.Day:
                return currentWeather switch
                {
                    WeatherType.Clear => (dayClearLightIntensity, dayClearLightColor),
                    WeatherType.Cloudy => (dayCloudyLightIntensity, dayCloudyLightColor),
                    WeatherType.Rain => (dayRainLightIntensity, dayRainLightColor),
                    WeatherType.Snow => (daySnowLightIntensity, daySnowLightColor),
                    _ => (dayClearLightIntensity, dayClearLightColor)
                };
            case TimeOfDay.Dusk:
                return currentWeather switch
                {
                    WeatherType.Clear => (duskClearLightIntensity, duskClearLightColor),
                    WeatherType.Cloudy => (duskCloudyLightIntensity, duskCloudyLightColor),
                    WeatherType.Rain => (duskRainLightIntensity, duskRainLightColor),
                    WeatherType.Snow => (duskSnowLightIntensity, duskSnowLightColor),
                    _ => (duskClearLightIntensity, duskClearLightColor)
                };
            case TimeOfDay.Night:
                return currentWeather switch
                {
                    WeatherType.Clear => (nightClearLightIntensity, nightClearLightColor),
                    WeatherType.Cloudy => (nightCloudyLightIntensity, nightCloudyLightColor),
                    WeatherType.Rain => (nightRainLightIntensity, nightRainLightColor),
                    WeatherType.Snow => (nightSnowLightIntensity, nightSnowLightColor),
                    _ => (nightClearLightIntensity, nightClearLightColor)
                };
            default:
                return (dayClearLightIntensity, dayClearLightColor);
        }
    }

    // 原有方法（无任何修改）
    private Material GetSkybox(TimeOfDay time, WeatherType weather)
    {
        switch (time)
        {
            case TimeOfDay.Day:
                switch (weather)
                {
                    case WeatherType.Clear: return dayClearSkybox;
                    case WeatherType.Cloudy: return dayCloudySkybox;
                    case WeatherType.Rain: return dayRainSkybox;
                    case WeatherType.Snow: return daySnowSkybox;
                }
                break;
            case TimeOfDay.Dusk:
                switch (weather)
                {
                    case WeatherType.Clear: return duskClearSkybox;
                    case WeatherType.Cloudy: return duskCloudySkybox;
                    case WeatherType.Rain: return duskRainSkybox;
                    case WeatherType.Snow: return duskSnowSkybox;
                }
                break;
            case TimeOfDay.Night:
                switch (weather)
                {
                    case WeatherType.Clear: return nightClearSkybox;
                    case WeatherType.Cloudy: return nightCloudySkybox;
                    case WeatherType.Rain: return nightRainSkybox;
                    case WeatherType.Snow: return nightSnowSkybox;
                }
                break;
        }
        return dayClearSkybox;
    }

    private void ControlParticles(WeatherType weather)
    {
        if (rainParticle != null)
        {
            if (!rainParticle.gameObject.activeInHierarchy)
                rainParticle.gameObject.SetActive(true);
            rainParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (snowParticle != null)
        {
            if (!snowParticle.gameObject.activeInHierarchy)
                snowParticle.gameObject.SetActive(true);
            snowParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        switch (weather)
        {
            case WeatherType.Rain:
                rainParticle?.Play();
                Debug.Log($"雨粒子开始播放 - 状态: {rainParticle.isPlaying}");
                break;
            case WeatherType.Snow:
                snowParticle?.Play();
                Debug.Log($"雪粒子开始播放 - 状态: {snowParticle.isPlaying}");
                break;
            default:
                if (rainParticle != null && rainParticle.gameObject.activeInHierarchy)
                    rainParticle.gameObject.SetActive(false);
                if (snowParticle != null && snowParticle.gameObject.activeInHierarchy)
                    snowParticle.gameObject.SetActive(false);
                break;
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            string particleInfo = currentWeather switch
            {
                WeatherType.Rain => " + Rain Effect",
                WeatherType.Snow => " + Snow Effect",
                _ => ""
            };

            string realTimeStatus = (realTimeToggle != null && realTimeToggle.isOn) ? " (Real-time Mode)" : "";

            statusText.text = $"Time: {currentTime}\nWeather: {currentWeather}{particleInfo}{realTimeStatus}\nUpdated: {DateTime.Now:HH:mm:ss}";
        }
    }

    // ========== 实时天气功能（无修改）==========
    private void OnRealDataUpdated(DateTime beijingTime, string weather, float temperature)
    {
        if (realTimeToggle == null || !realTimeToggle.isOn) return;

        Debug.Log($"Real data updated: {beijingTime}, {weather}, {temperature}");

        string timeOfDay = realTimeFetcher.GetTimeOfDay();
        TimeOfDay mappedTime = timeOfDay switch
        {
            "Day" => TimeOfDay.Day,
            "Dusk" => TimeOfDay.Dusk,
            "Night" => TimeOfDay.Night,
            _ => TimeOfDay.Day
        };

        string mappedWeatherStr = realTimeFetcher.GetMappedWeatherType();
        WeatherType mappedWeather = mappedWeatherStr switch
        {
            "Rain" => WeatherType.Rain,
            "Snow" => WeatherType.Snow,
            "Cloudy" => WeatherType.Cloudy,
            _ => WeatherType.Clear
        };

        currentTime = mappedTime;
        currentWeather = mappedWeather;
        ApplyCurrentSettings();
        UpdateStatusWithRealData(beijingTime, weather, temperature);
    }

    private void UpdateStatusWithRealData(DateTime beijingTime, string weather, float temperature)
    {
        if (statusText != null)
        {
            string realDataInfo = $"\nReal Data: {beijingTime:HH:mm} {weather} {temperature}";
            string particleInfo = currentWeather switch
            {
                WeatherType.Rain => " + Rain Effect",
                WeatherType.Snow => " + Snow Effect",
                _ => ""
            };

            statusText.text = $"Time: {currentTime}\nWeather: {currentWeather}{particleInfo}{realDataInfo}";
        }
    }

    private void OnRealTimeToggleChanged(bool isOn)
    {
        UpdateRealTimeToggleText();

        if (isOn && realTimeFetcher != null && realTimeFetcher.IsDataValid())
        {
            ApplyRealTimeData();
        }
        else if (isOn && realTimeFetcher != null)
        {
            realTimeFetcher.ManualRefresh();
        }
    }

    private void RefreshRealTimeData()
    {
        if (realTimeFetcher != null)
        {
            realTimeFetcher.ManualRefresh();
        }
    }

    private void ApplyRealTimeData()
    {
        if (realTimeFetcher != null && realTimeFetcher.IsDataValid())
        {
            string timeOfDay = realTimeFetcher.GetTimeOfDay();
            TimeOfDay mappedTime = timeOfDay switch
            {
                "Day" => TimeOfDay.Day,
                "Dusk" => TimeOfDay.Dusk,
                "Night" => TimeOfDay.Night,
                _ => TimeOfDay.Day
            };

            string mappedWeatherStr = realTimeFetcher.GetMappedWeatherType();
            WeatherType mappedWeather = mappedWeatherStr switch
            {
                "Rain" => WeatherType.Rain,
                "Snow" => WeatherType.Snow,
                "Cloudy" => WeatherType.Cloudy,
                _ => WeatherType.Clear
            };

            currentTime = mappedTime;
            currentWeather = mappedWeather;
            ApplyCurrentSettings();
        }
    }

    private void UpdateRealTimeToggleText()
    {
        if (realTimeToggle != null)
        {
            TextMeshProUGUI toggleText = realTimeToggle.GetComponentInChildren<TextMeshProUGUI>();
            if (toggleText != null)
            {
                toggleText.text = realTimeToggle.isOn ? "Real-time: ON" : "Real-time: OFF";
            }
        }
    }

    void OnDestroy()
    {
        if (realTimeFetcher != null)
        {
            realTimeFetcher.OnRealDataUpdated -= OnRealDataUpdated;
        }

        if (realTimeToggle != null)
        {
            realTimeToggle.onValueChanged.RemoveListener(OnRealTimeToggleChanged);
        }
    }

    // 原有调试方法（无修改）
    [ContextMenu("Set Day Clear")]
    public void DebugDayClear()
    {
        if (realTimeToggle != null) realTimeToggle.isOn = false;
        currentTime = TimeOfDay.Day;
        currentWeather = WeatherType.Clear;
        ApplyCurrentSettings();
    }

    [ContextMenu("Set Day Cloudy")]
    public void DebugDayCloudy()
    {
        if (realTimeToggle != null) realTimeToggle.isOn = false;
        currentTime = TimeOfDay.Day;
        currentWeather = WeatherType.Cloudy;
        ApplyCurrentSettings();
    }

    [ContextMenu("Set Day Rain")]
    public void DebugDayRain()
    {
        if (realTimeToggle != null) realTimeToggle.isOn = false;
        currentTime = TimeOfDay.Day;
        currentWeather = WeatherType.Rain;
        ApplyCurrentSettings();
    }

    [ContextMenu("Set Night Snow")]
    public void DebugNightSnow()
    {
        if (realTimeToggle != null) realTimeToggle.isOn = false;
        currentTime = TimeOfDay.Night;
        currentWeather = WeatherType.Snow;
        ApplyCurrentSettings();
    }
}