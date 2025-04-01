namespace WebMVC.Services.ModelDTOs;

/// <summary>
/// 地理位置数据传输对象
/// </summary>
public record LocationDTO
{
    /// <summary>
    /// 经度值
    /// </summary>
    public double Longitude { get; init; }

    /// <summary>
    /// 纬度值
    /// </summary>
    public double Latitude { get; init; }
}
