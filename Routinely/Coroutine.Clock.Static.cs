namespace Routinely;

public partial struct Coroutine
{
    public static class Clock
    {
        public static double Delta
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => StackDispatcher.DeltaSeconds;
        }

        public static TimeSpan DeltaTimeSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => TimeSpan.FromSeconds(StackDispatcher.DeltaSeconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update(TimeSpan delta) => Update(delta.TotalSeconds);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update(double seconds) => StackDispatcher.UpdateDelta(seconds);

        public static async Coroutine Delay(TimeSpan timeSpan)
            => await Delay(timeSpan.TotalSeconds);

        public static async Coroutine Delay(double seconds)
        {
            if(seconds == 0)
            {
                return;
            }

            if (!IsPositiveFinite(seconds))
            {
                CoroutineClockException.ThrowDelayMustBeZeroOrPositiveSeconds();
            }

            var startTime = TotalElapsedSeconds;
            var previousTime = startTime;

            while (TotalElapsedSeconds - startTime < seconds)
            {
                await Yield();

                var currentTime = TotalElapsedSeconds;
                if (currentTime == previousTime)
                {
                    CoroutineClockException.ThrowClockNotUpdated();
                }

                previousTime = currentTime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsPositiveFinite(double value) => value > 0 && !double.IsInfinity(value) && !double.IsNaN(value);
    }
}