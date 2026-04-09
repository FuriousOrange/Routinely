namespace Routinely;

public static class CoroutineNotifyAwaitingExtensions
{
    /// <summary>
    /// Enforces a <see cref="CancellationContract"/> on an awaiting operation.
    /// Ensures that if a coroutine is canceled while awaiting a cancellation exception will be thrown.
    /// Allows for proper cleanup in finally blocks.
    /// </summary>
    /// <typeparam name="TAwaiter"></typeparam>
    /// <param name="awaiter">The awaiter to enforce the cancellation contract on.</param>
    /// <param name="contract">The cancellation contract to enforce.</param>
    /// <returns>The awaiter with the enforced cancellation contract.</returns>
    /// <remarks>
    /// Use in conjunction with <see cref="CancellationContract.Enter"/> to ensure that cancellation is properly observed.
    /// <code>
    /// async Coroutine WorkWithResources()
    /// {
    ///     using var ctc = await CancellationContract.Enter();
    ///     
    ///     try
    ///     {
    ///         // Load resources
    ///         var data = await LoadData().Enforce(ctc); // Enforced - will cancel
    ///         
    ///         await Coroutine.Yield().Enforce(ctc); // Enforced - will cancel
    ///         
    ///         // Process data
    ///     }
    ///     finally
    ///     {
    ///         // Always runs - cleanup resources here
    ///         CleanupResources();
    ///     }
    /// }
    /// </code>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TAwaiter Enforce<TAwaiter>
        (this TAwaiter awaiter, CancellationContract contract)
        where TAwaiter : ICoroutineNotifyAwaiting
    {
        contract.Enforce();

        return awaiter;
    }
}
