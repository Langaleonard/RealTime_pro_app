using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RealTimeProApp.Models;

public class CaptureViewModel
{
    [Required]
    [Display(Name = "Sensor ID")]
    public string SensorId { get; set; } = "SENSOR-001";

    [Required]
    [Display(Name = "Field / Zone")]
    public string FieldName { get; set; } = "Field A";

    [Range(0, 100)]
    [Display(Name = "Soil Moisture (%)")]
    public double SoilMoisture { get; set; } = 45;

    [Range(-20, 80)]
    [Display(Name = "Temperature (°C)")]
    public double Temperature { get; set; } = 24;

    [Display(Name = "Drone / Field Note")]
    public string DroneNote { get; set; } = string.Empty;

    [Display(Name = "Drone Image")]
    public IFormFile? DroneImage { get; set; }
}
