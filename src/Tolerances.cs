public class Tolerances
{
    public const double DEF_DARK_START = 0;

    public const double DEF_SHADOW_START = .2;

    public const double DEF_MID_START = .4;

    public const double DEF_BRIGHT_START = .6;

    public Tolerances()
    {
        Darks = new(valueStart:DEF_DARK_START);
        Shadows = new(valueStart:DEF_SHADOW_START);
        Midtones = new(valueStart:DEF_MID_START);
        Brights = new(valueStart:DEF_BRIGHT_START);
    }

    public ThresholdHSV Darks { get; set; }

    public ThresholdHSV Shadows { get; set; }

    public ThresholdHSV Midtones { get; set; }

    public ThresholdHSV Brights { get; set; }

    /// <summary>
    /// Get threshold for dark, shadow, midtone, or bright values.
    /// </summary>
    /// 
    /// <param name="value">Value component of color to compare.</param>
    public ThresholdHSV GetThreshold(double value)
    {
        if (value >= Shadows.ValueStart && value < Midtones.ValueStart)
        {
            return Shadows;
        }
        else if (value >= Midtones.ValueStart && value < Brights.ValueStart)
        {
            return Midtones;
        }
        else if (value >= Brights.ValueStart)
        {
            return Brights;
        }

        return Darks;
    }

    public override string ToString()
    {
        return $"Darks: {Darks}\nShadows: {Shadows}\nMidtones: {Midtones}\nBrights: {Brights}";
    }
}