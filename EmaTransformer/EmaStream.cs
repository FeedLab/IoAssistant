namespace IoAssistant.Transformers;

public class EmaStream
{
    private readonly decimal alpha;   // responsiveness factor
    private readonly Queue<decimal> seedBuffer = new();
    private readonly int period;
    private decimal? lastEma;

    /// <summary>
    /// Create an EMA stream.
    /// </summary>
    /// <param name="period">Number of points to seed SMA.</param>
    /// <param name="alpha">Responsiveness factor (0 < alpha <= 1).
    /// Higher alpha = faster response, lower alpha = smoother.</param>
    public EmaStream(int period, decimal alpha)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be greater than zero.", nameof(period));
        if (alpha <= 0 || alpha > 1)
            throw new ArgumentException("Alpha must be between 0 and 1.", nameof(alpha));

        this.period = period;
        this.alpha = alpha;
    }

    public decimal AddPoint(decimal newValue)
    {
        if (lastEma == null)
        {
            seedBuffer.Enqueue(newValue);

            if (seedBuffer.Count < period)
            {
                return seedBuffer.Average();
            }
            else
            {
                lastEma = seedBuffer.Average();
                return lastEma.Value;
            }
        }
        else
        {
            lastEma = (newValue * alpha) + (lastEma.Value * (1 - alpha));
            return lastEma.Value;
        }
    }

    public decimal? Current => lastEma;
}
