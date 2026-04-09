namespace Routinely;

public partial class CancellationContract
{
    /// <summary>
    /// Enters a cancellation contract scope for safe resource cleanup on coroutine cancellation.
    /// Must be used with 'using' statement to ensure proper disposal.
    /// </summary>
    /// <returns>
    /// A pooled <see cref="CancellationContract"/> that tracks cancellation state.
    /// The contract is automatically returned to the pool when disposed.
    /// </returns>
    /// <remarks>
    /// A cancellation contract enables structured cleanup when a coroutine is cancelled.
    /// The contract is scoped to the requesting coroutine and should not be passed to external scopes.
    /// <para>
    /// When a coroutine with an active contract is cancelled, awaits with <c>.Enforce(ctc)</c> will
    /// throw a cancellation exception, allowing cleanup logic in finally blocks to execute.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> You must enforce the contract at every async operation using <c>.Enforce(ctc)</c>.
    /// Unenforced operations will not be interrupted on cancellation.
    /// </para>
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
    public static CancellationContract Enter()
        => Exchange<CancellationContract>
            .Reserve()
            .BindOnNull(static token => new(token));
    
}
