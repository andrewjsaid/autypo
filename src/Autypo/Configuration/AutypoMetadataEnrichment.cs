namespace Autypo.Configuration;

/// <summary>
/// Specifies optional metadata that can be collected and exposed during the search process.
/// </summary>
/// <remarks>
/// Enabling enrichment allows Autypo to emit additional information during query execution,
/// which can be consumed by advanced components such as scorers, filters, or diagnostics.
///
/// These options are off by default to reduce memory and CPU overhead in common scenarios.
/// </remarks>
[Flags]
public enum AutypoMetadataEnrichment
{
    /// <summary>
    /// Includes the transformed tokens used during indexing for each candidate document.
    /// </summary>
    /// <remarks>
    /// This enables downstream components to access the token text that was stored and matched,
    /// via <see cref="MatchCandidate{T}.GetExtractedDocumentTokenInfo(int)"/>. This is useful for:
    /// <list type="bullet">
    ///   <item>Token-aware scoring strategies</item>
    ///   <item>Advanced filtering based on matched tokens</item>
    ///   <item>Debugging or diagnostics of the tokenization pipeline</item>
    /// </list>
    /// <para>
    /// Enabling this incurs additional memory and processing cost during indexing and matching,
    /// and should only be used when such metadata is explicitly required.
    /// </para>
    /// </remarks>
    IncludeDocumentTokenText = 1,
}
