using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IMPORTANT: The TelemetryWrapper class is used to handle the additional
// wrapper layer in the received JSON string. The JSON structure includes
// an outer layer named "pr_telemetry," and the TelemetryWrapper helps to
// unwrap and deserialize the data within that layer.
[System.Serializable]
public class ROVER_TELEMETRYWrapper
{
    // The pr_telemetry field represents the inner data structure containing
    // the actual telemetry information we want to extract and use.
    public ROVER_TELEMETRYData pr_telemetry;
}

[System.Serializable]
public class ROVER_TELEMETRYData
{
    public bool ac_heating;
    public bool ac_cooling;
    public bool co2_scrubber;
    public bool lights_on;
    public bool internal_lights_on;
    public bool brakes;
    public bool in_sunlight;
    public float throttle;
    public float steering;
    public float current_pos_x;
    public float current_pos_y;
    public float current_pos_alt;
    public float heading;
    public float pitch;
    public float roll;
    public float distance_traveled;
    public float speed;
    public float surface_incline;
    public float oxygen_tank;
    public float oxygen_pressure;
    public float oxygen_levels;
    public bool fan_pri;
    public int ac_fan_pri;
    public int ac_fan_sec;
    public float cabin_pressure;
    public float cabin_temperature;
    public float battery_level;
    public float power_consumption_rate;
    public float solar_panel_efficiency;
    public float external_temp;
    public float pr_coolant_level;
    public float pr_coolant_pressure;
    public float pr_coolant_tank;
    public float radiator;
    public float motor_power_consumption;
    public float terrain_condition;
    public float solar_panel_dust_accum;
    public float mission_elapsed_time;
    public float mission_planned_time;
    public float point_of_no_return;
    public float distance_from_base;
    public bool switch_dest;
    public float dest_x;
    public float dest_y;
    public float dest_z;
    public bool dust_wiper;
    public float[] lidar;
}

public class ROVER_TELEMETRYDataHandler : MonoBehaviour
{
    // Reference to the TSScDataHandler script
    public TSScDataHandler TSS;
    public ROVER_TELEMETRYWrapper rover_telemetryWrapper;

    // Start is called before the first frame update
    void Start()
    {
        rover_telemetryWrapper = new ROVER_TELEMETRYWrapper();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateROVER_TELEMETRYData();
    }

    void UpdateROVER_TELEMETRYData()
    {
        // Store the JSON string for parsing
        string rover_telemetry_json_string = TSS.GetROVER_TELEMETRYData();

        // Parse JSON string into ROVER_TELEMETRYWrapper object
        // This stores the JSON value into the ROVER_TELEMETRYWrapper object
        rover_telemetryWrapper = JsonUtility.FromJson<ROVER_TELEMETRYWrapper>(rover_telemetry_json_string);

        // Uncomment for debugging
        // Debug.Log($"Battery Level: {GetBatteryLevel()}");
        // Debug.Log($"Oxygen Pressure: {GetOxygenPressure()}");
        // Debug.Log($"Cabin Temperature: {GetCabinTemperature()}");
    }

    // Environmental systems
    public bool GetACHeating()
    {
        return rover_telemetryWrapper.pr_telemetry.ac_heating;
    }

    public bool GetACCooling()
    {
        return rover_telemetryWrapper.pr_telemetry.ac_cooling;
    }

    public bool GetCO2Scrubber()
    {
        return rover_telemetryWrapper.pr_telemetry.co2_scrubber;
    }

    public float GetCabinTemperature()
    {
        return rover_telemetryWrapper.pr_telemetry.cabin_temperature;
    }

    public float GetCabinPressure()
    {
        return rover_telemetryWrapper.pr_telemetry.cabin_pressure;
    }

    public float GetExternalTemperature()
    {
        return rover_telemetryWrapper.pr_telemetry.external_temp;
    }

    // Power systems
    public float GetBatteryLevel()
    {
        return rover_telemetryWrapper.pr_telemetry.battery_level;
    }

    public float GetPowerConsumptionRate()
    {
        return rover_telemetryWrapper.pr_telemetry.power_consumption_rate;
    }

    public float GetSolarPanelEfficiency()
    {
        return rover_telemetryWrapper.pr_telemetry.solar_panel_efficiency;
    }

    public float GetSolarPanelDustAccumulation()
    {
        return rover_telemetryWrapper.pr_telemetry.solar_panel_dust_accum;
    }

    public bool GetInSunlight()
    {
        return rover_telemetryWrapper.pr_telemetry.in_sunlight;
    }

    // Oxygen systems
    public float GetOxygenTank()
    {
        return rover_telemetryWrapper.pr_telemetry.oxygen_tank;
    }

    public float GetOxygenPressure()
    {
        return rover_telemetryWrapper.pr_telemetry.oxygen_pressure;
    }

    public float GetOxygenLevels()
    {
        return rover_telemetryWrapper.pr_telemetry.oxygen_levels;
    }

    // Fan systems
    public bool GetFanPrimary()
    {
        return rover_telemetryWrapper.pr_telemetry.fan_pri;
    }

    public int GetFanPrimaryRPM()
    {
        return rover_telemetryWrapper.pr_telemetry.ac_fan_pri;
    }

    public int GetFanSecondaryRPM()
    {
        return rover_telemetryWrapper.pr_telemetry.ac_fan_sec;
    }

    // Coolant systems
    public float GetCoolantLevel()
    {
        return rover_telemetryWrapper.pr_telemetry.pr_coolant_level;
    }

    public float GetCoolantPressure()
    {
        return rover_telemetryWrapper.pr_telemetry.pr_coolant_pressure;
    }

    public float GetCoolantTank()
    {
        return rover_telemetryWrapper.pr_telemetry.pr_coolant_tank;
    }

    public float GetRadiator()
    {
        return rover_telemetryWrapper.pr_telemetry.radiator;
    }

    // Navigation and positioning
    public float GetPositionX()
    {
        return rover_telemetryWrapper.pr_telemetry.current_pos_x;
    }

    public float GetPositionY()
    {
        return rover_telemetryWrapper.pr_telemetry.current_pos_y;
    }

    public float GetAltitude()
    {
        return rover_telemetryWrapper.pr_telemetry.current_pos_alt;
    }

    public float GetHeading()
    {
        return rover_telemetryWrapper.pr_telemetry.heading;
    }

    public float GetPitch()
    {
        return rover_telemetryWrapper.pr_telemetry.pitch;
    }

    public float GetRoll()
    {
        return rover_telemetryWrapper.pr_telemetry.roll;
    }

    public float GetSpeed()
    {
        return rover_telemetryWrapper.pr_telemetry.speed;
    }

    public float GetDistanceTraveled()
    {
        return rover_telemetryWrapper.pr_telemetry.distance_traveled;
    }

    public float GetDistanceFromBase()
    {
        return rover_telemetryWrapper.pr_telemetry.distance_from_base;
    }

    public float GetSurfaceIncline()
    {
        return rover_telemetryWrapper.pr_telemetry.surface_incline;
    }

    // Destination information
    public bool GetSwitchDest()
    {
        return rover_telemetryWrapper.pr_telemetry.switch_dest;
    }

    public float GetDestX()
    {
        return rover_telemetryWrapper.pr_telemetry.dest_x;
    }

    public float GetDestY()
    {
        return rover_telemetryWrapper.pr_telemetry.dest_y;
    }

    public float GetDestZ()
    {
        return rover_telemetryWrapper.pr_telemetry.dest_z;
    }

    // Mission timing
    public float GetMissionElapsedTime()
    {
        return rover_telemetryWrapper.pr_telemetry.mission_elapsed_time;
    }

    public float GetMissionPlannedTime()
    {
        return rover_telemetryWrapper.pr_telemetry.mission_planned_time;
    }

    public float GetPointOfNoReturn()
    {
        return rover_telemetryWrapper.pr_telemetry.point_of_no_return;
    }

    // Other systems
    public bool GetLightsOn()
    {
        return rover_telemetryWrapper.pr_telemetry.lights_on;
    }

    public bool GetInternalLightsOn()
    {
        return rover_telemetryWrapper.pr_telemetry.internal_lights_on;
    }

    public bool GetBrakes()
    {
        return rover_telemetryWrapper.pr_telemetry.brakes;
    }

    public float GetThrottle()
    {
        return rover_telemetryWrapper.pr_telemetry.throttle;
    }

    public float GetSteering()
    {
        return rover_telemetryWrapper.pr_telemetry.steering;
    }

    public bool GetDustWiper()
    {
        return rover_telemetryWrapper.pr_telemetry.dust_wiper;
    }

    public float GetMotorPowerConsumption()
    {
        return rover_telemetryWrapper.pr_telemetry.motor_power_consumption;
    }

    public float GetTerrainCondition()
    {
        return rover_telemetryWrapper.pr_telemetry.terrain_condition;
    }

    // LIDAR data
    public float[] GetLidarData()
    {
        return rover_telemetryWrapper.pr_telemetry.lidar;
    }
}