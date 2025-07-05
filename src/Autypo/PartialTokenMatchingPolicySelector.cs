using Autypo.Tokenization;

namespace Autypo;

/// <summary>
/// Delegate that determines the partial match policy for a query at runtime.
/// </summary>
/// <param name="tokens">
/// The transformed query tokens, after tokenization and any token transformations.
/// </param>
/// <param name="queryContext">
/// The query context, which includes tokenized input, metadata, and index information.
/// </param>
/// <returns>
/// A <see cref="PartialMatchPolicy"/> describing which tokens must match, or how many.
/// </returns>
public delegate PartialMatchPolicy PartialTokenMatchingPolicySelector(ReadOnlySpan<AutypoToken> tokens, AutypoQueryContext queryContext);
